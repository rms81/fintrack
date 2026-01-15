# Profiles Module

## Overview
Profiles provide logical separation between Personal and Business finances within a single user account.

## User Stories

| ID | As a... | I want to... | So that... |
|----|---------|--------------|------------|
| US-P1 | User | create a Personal profile | I can track my personal expenses separately |
| US-P2 | Freelancer | create a Business profile | I can track business income and expenses for tax purposes |
| US-P3 | User | switch between profiles | I can view and manage different financial contexts |
| US-P4 | User | rename my profile | I can better organize my financial data |
| US-P5 | User | delete a profile I no longer need | I can keep my account clean |
| US-P6 | User | have default categories seeded when creating a profile | I can start categorizing immediately |

## Domain Model

### Profile Entity
```csharp
public class Profile : BaseEntity
{
    public Guid UserId { get; init; }
    public required string Name { get; set; }
    public ProfileType Type { get; init; }
    
    // Navigation
    public ApplicationUser User { get; init; } = null!;
    public ICollection<Account> Accounts { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<CategorizationRule> Rules { get; set; } = [];
}

public enum ProfileType
{
    Personal,
    Business
}
```

### Value Objects
```csharp
public record ProfileName
{
    public string Value { get; }
    
    public ProfileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Profile name is required");
        if (value.Length > 100)
            throw new DomainException("Profile name must be 100 characters or less");
        Value = value.Trim();
    }
    
    public static implicit operator string(ProfileName name) => name.Value;
}
```

## Database

### Table: profiles
```sql
CREATE TABLE profiles (
    id uuid PRIMARY KEY DEFAULT uuidv7(),
    user_id uuid NOT NULL REFERENCES asp_net_users(id) ON DELETE CASCADE,
    name varchar(100) NOT NULL,
    type varchar(20) NOT NULL CHECK (type IN ('personal', 'business')),
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_profiles_user_id ON profiles(user_id);
CREATE UNIQUE INDEX ix_profiles_user_name ON profiles(user_id, name);
```

### EF Configuration
```csharp
public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("uuidv7()");
        
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.HasOne(x => x.User)
            .WithMany(u => u.Profiles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
    }
}
```

## Endpoints

### GET /api/profiles
List all profiles for current user.

**Response:** `200 OK`
```json
[
  {
    "id": "0193a1b2-...",
    "name": "Personal",
    "type": "personal",
    "accountCount": 2,
    "createdAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": "0193a1b3-...",
    "name": "Freelance Business",
    "type": "business",
    "accountCount": 1,
    "createdAt": "2024-01-15T10:35:00Z"
  }
]
```

### GET /api/profiles/{id}
Get single profile details.

**Response:** `200 OK`
```json
{
  "id": "0193a1b2-...",
  "name": "Personal",
  "type": "personal",
  "accountCount": 2,
  "categoryCount": 15,
  "transactionCount": 1250,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T14:00:00Z"
}
```

### POST /api/profiles
Create new profile.

**Request:**
```json
{
  "name": "Side Business",
  "type": "business"
}
```

**Response:** `201 Created`
```json
{
  "id": "0193a1b4-...",
  "name": "Side Business",
  "type": "business",
  "createdAt": "2024-01-21T09:00:00Z"
}
```

### PUT /api/profiles/{id}
Update profile.

**Request:**
```json
{
  "name": "Main Business"
}
```

**Response:** `200 OK`

### DELETE /api/profiles/{id}
Delete profile and all associated data.

**Response:** `204 No Content`

## Wolverine Handlers

### Commands
```csharp
public record CreateProfile(string Name, ProfileType Type);
public record UpdateProfile(Guid Id, string Name);
public record DeleteProfile(Guid Id);
```

### Queries
```csharp
public record GetProfiles;
public record GetProfileById(Guid Id);
```

### CreateProfile Handler
```csharp
public static class CreateProfileHandler
{
    public static async Task<ProfileDto> HandleAsync(
        CreateProfile command,
        FinTrackDbContext db,
        IUserContext userContext,
        CancellationToken ct)
    {
        var userId = userContext.UserId;
        
        // Check for duplicate name
        var exists = await db.Profiles
            .AnyAsync(p => p.UserId == userId && p.Name == command.Name, ct);
            
        if (exists)
            throw new ConflictException($"Profile '{command.Name}' already exists");
        
        var profile = new Profile
        {
            UserId = userId,
            Name = command.Name,
            Type = command.Type
        };
        
        db.Profiles.Add(profile);
        await db.SaveChangesAsync(ct);
        
        // Create default categories for new profile
        await CreateDefaultCategories(db, profile.Id, ct);
        
        return profile.ToDto();
    }
    
    private static async Task CreateDefaultCategories(
        FinTrackDbContext db, 
        Guid profileId, 
        CancellationToken ct)
    {
        var defaults = new[]
        {
            new Category { ProfileId = profileId, Name = "Food & Dining", Icon = "utensils", Color = "#FF6B6B" },
            new Category { ProfileId = profileId, Name = "Transportation", Icon = "car", Color = "#4ECDC4" },
            new Category { ProfileId = profileId, Name = "Shopping", Icon = "shopping-bag", Color = "#45B7D1" },
            new Category { ProfileId = profileId, Name = "Bills & Utilities", Icon = "file-text", Color = "#96CEB4" },
            new Category { ProfileId = profileId, Name = "Entertainment", Icon = "film", Color = "#FFEAA7" },
            new Category { ProfileId = profileId, Name = "Income", Icon = "dollar-sign", Color = "#26DE81" },
            new Category { ProfileId = profileId, Name = "Other", Icon = "more-horizontal", Color = "#A0A0A0" },
        };
        
        db.Categories.AddRange(defaults);
        await db.SaveChangesAsync(ct);
    }
}
```

### GetProfiles Handler
```csharp
public static class GetProfilesHandler
{
    public static async Task<IReadOnlyList<ProfileListDto>> HandleAsync(
        GetProfiles query,
        FinTrackDbContext db,
        IUserContext userContext,
        CancellationToken ct)
    {
        return await db.Profiles
            .Where(p => p.UserId == userContext.UserId)
            .OrderBy(p => p.Name)
            .Select(p => new ProfileListDto(
                p.Id,
                p.Name,
                p.Type,
                p.Accounts.Count,
                p.CreatedAt
            ))
            .ToListAsync(ct);
    }
}
```

## DTOs

```csharp
public record ProfileListDto(
    Guid Id,
    string Name,
    ProfileType Type,
    int AccountCount,
    DateTimeOffset CreatedAt
);

public record ProfileDto(
    Guid Id,
    string Name,
    ProfileType Type,
    int AccountCount,
    int CategoryCount,
    int TransactionCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CreateProfileRequest(string Name, ProfileType Type);
public record UpdateProfileRequest(string Name);
```

## React Components

### Profile List
```typescript
// src/features/profiles/ProfileList.tsx
export function ProfileList() {
  const { data: profiles, isLoading } = useProfiles();
  
  if (isLoading) return <ProfileListSkeleton />;
  
  return (
    <div className="grid gap-4 md:grid-cols-2">
      {profiles?.map(profile => (
        <ProfileCard key={profile.id} profile={profile} />
      ))}
      <CreateProfileCard />
    </div>
  );
}
```

### Profile Selector (Header)
```typescript
// src/features/profiles/ProfileSelector.tsx
export function ProfileSelector() {
  const { currentProfile, setCurrentProfile } = useProfileContext();
  const { data: profiles } = useProfiles();
  
  return (
    <Select value={currentProfile?.id} onValueChange={setCurrentProfile}>
      {profiles?.map(p => (
        <SelectItem key={p.id} value={p.id}>
          {p.name}
        </SelectItem>
      ))}
    </Select>
  );
}
```

## Testing

```csharp
[Fact]
public async Task CreateProfile_WithValidData_ReturnsCreated()
{
    // Arrange
    await AuthenticateAsync();
    var request = new { name = "Test Profile", type = "personal" };
    
    // Act
    var response = await Client.PostAsJsonAsync("/api/profiles", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
    profile!.Name.Should().Be("Test Profile");
}

[Fact]
public async Task CreateProfile_WithDuplicateName_ReturnsConflict()
{
    // Arrange
    await AuthenticateAsync();
    await CreateProfileAsync("Existing");
    var request = new { name = "Existing", type = "personal" };
    
    // Act
    var response = await Client.PostAsJsonAsync("/api/profiles", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
}
```

## Business Rules

1. Each user can have multiple profiles
2. Profile names must be unique per user
3. Deleting a profile cascades to all accounts, categories, transactions, and rules
4. New profiles get default categories automatically
5. A user must have at least one profile (enforce in UI, not backend)
