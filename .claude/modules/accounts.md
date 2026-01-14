# Accounts Module

## Overview
Accounts represent bank accounts or financial sources linked to a profile. They serve as the target for CSV imports and group transactions.

## Domain Model

### Account Entity
```csharp
public class Account : BaseEntity
{
    public Guid ProfileId { get; init; }
    public required string Name { get; set; }
    public required string Institution { get; set; }
    public string? AccountNumberMasked { get; set; }
    public required Currency Currency { get; init; }
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public Profile Profile { get; init; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<ImportSession> ImportSessions { get; set; } = [];
}

public enum Currency
{
    EUR,
    USD,
    GBP,
    CHF,
    // Add more as needed
}
```

### Value Objects
```csharp
public record MaskedAccountNumber
{
    public string Value { get; }
    
    public MaskedAccountNumber(string fullNumber)
    {
        if (string.IsNullOrWhiteSpace(fullNumber) || fullNumber.Length < 4)
        {
            Value = "****";
            return;
        }
        Value = $"****{fullNumber[^4..]}";
    }
    
    public static implicit operator string(MaskedAccountNumber masked) => masked.Value;
}
```

## Database

### Table: accounts
```sql
CREATE TABLE accounts (
    id uuid PRIMARY KEY DEFAULT uuidv7(),
    profile_id uuid NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    name varchar(100) NOT NULL,
    institution varchar(100) NOT NULL,
    account_number_masked varchar(20),
    currency char(3) NOT NULL DEFAULT 'EUR',
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_accounts_profile_id ON accounts(profile_id);
CREATE UNIQUE INDEX ix_accounts_profile_name ON accounts(profile_id, name);
```

### EF Configuration
```csharp
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("uuidv7()");
        
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Institution).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AccountNumberMasked).HasMaxLength(20);
        builder.Property(x => x.Currency)
            .HasConversion<string>()
            .HasMaxLength(3);
            
        builder.HasOne(x => x.Profile)
            .WithMany(p => p.Accounts)
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(x => new { x.ProfileId, x.Name }).IsUnique();
    }
}
```

## Endpoints

### GET /api/profiles/{profileId}/accounts
List all accounts for a profile.

**Response:** `200 OK`
```json
[
  {
    "id": "0193a1c1-...",
    "name": "Main Checking",
    "institution": "Millennium BCP",
    "accountNumberMasked": "****4521",
    "currency": "EUR",
    "isActive": true,
    "transactionCount": 450,
    "lastImport": "2024-01-20T14:30:00Z"
  }
]
```

### GET /api/accounts/{id}
Get single account details.

**Response:** `200 OK`
```json
{
  "id": "0193a1c1-...",
  "profileId": "0193a1b2-...",
  "name": "Main Checking",
  "institution": "Millennium BCP",
  "accountNumberMasked": "****4521",
  "currency": "EUR",
  "isActive": true,
  "transactionCount": 450,
  "balance": 2450.75,
  "lastImport": "2024-01-20T14:30:00Z",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T14:30:00Z"
}
```

### POST /api/profiles/{profileId}/accounts
Create new account.

**Request:**
```json
{
  "name": "Savings Account",
  "institution": "Caixa Geral de Depósitos",
  "accountNumber": "PT50000201231234567890154",
  "currency": "EUR"
}
```

**Response:** `201 Created`
```json
{
  "id": "0193a1c2-...",
  "name": "Savings Account",
  "institution": "Caixa Geral de Depósitos",
  "accountNumberMasked": "****0154",
  "currency": "EUR",
  "isActive": true
}
```

### PUT /api/accounts/{id}
Update account.

**Request:**
```json
{
  "name": "Primary Savings",
  "institution": "CGD",
  "isActive": true
}
```

**Response:** `200 OK`

### DELETE /api/accounts/{id}
Delete account and all transactions.

**Response:** `204 No Content`

## Wolverine Handlers

### Commands
```csharp
public record CreateAccount(
    Guid ProfileId,
    string Name,
    string Institution,
    string? AccountNumber,
    Currency Currency
);

public record UpdateAccount(
    Guid Id,
    string Name,
    string Institution,
    bool IsActive
);

public record DeleteAccount(Guid Id);
```

### Queries
```csharp
public record GetAccountsByProfile(Guid ProfileId);
public record GetAccountById(Guid Id);
```

### CreateAccount Handler
```csharp
public static class CreateAccountHandler
{
    public static async Task<AccountDto> HandleAsync(
        CreateAccount command,
        FinTrackDbContext db,
        IUserContext userContext,
        CancellationToken ct)
    {
        // Verify profile belongs to user
        var profile = await db.Profiles
            .FirstOrDefaultAsync(p => 
                p.Id == command.ProfileId && 
                p.UserId == userContext.UserId, ct);
                
        if (profile is null)
            throw new NotFoundException("Profile not found");
        
        // Check for duplicate name
        var exists = await db.Accounts
            .AnyAsync(a => 
                a.ProfileId == command.ProfileId && 
                a.Name == command.Name, ct);
                
        if (exists)
            throw new ConflictException($"Account '{command.Name}' already exists");
        
        var account = new Account
        {
            ProfileId = command.ProfileId,
            Name = command.Name,
            Institution = command.Institution,
            AccountNumberMasked = command.AccountNumber is not null 
                ? new MaskedAccountNumber(command.AccountNumber).Value 
                : null,
            Currency = command.Currency
        };
        
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);
        
        return account.ToDto();
    }
}
```

### GetAccountsByProfile Handler
```csharp
public static class GetAccountsByProfileHandler
{
    public static async Task<IReadOnlyList<AccountListDto>> HandleAsync(
        GetAccountsByProfile query,
        FinTrackDbContext db,
        IUserContext userContext,
        CancellationToken ct)
    {
        // Verify profile access
        var hasAccess = await db.Profiles
            .AnyAsync(p => 
                p.Id == query.ProfileId && 
                p.UserId == userContext.UserId, ct);
                
        if (!hasAccess)
            throw new NotFoundException("Profile not found");
        
        return await db.Accounts
            .Where(a => a.ProfileId == query.ProfileId)
            .OrderBy(a => a.Name)
            .Select(a => new AccountListDto(
                a.Id,
                a.Name,
                a.Institution,
                a.AccountNumberMasked,
                a.Currency,
                a.IsActive,
                a.Transactions.Count,
                a.ImportSessions
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => (DateTimeOffset?)i.CreatedAt)
                    .FirstOrDefault()
            ))
            .ToListAsync(ct);
    }
}
```

## DTOs

```csharp
public record AccountListDto(
    Guid Id,
    string Name,
    string Institution,
    string? AccountNumberMasked,
    Currency Currency,
    bool IsActive,
    int TransactionCount,
    DateTimeOffset? LastImport
);

public record AccountDto(
    Guid Id,
    Guid ProfileId,
    string Name,
    string Institution,
    string? AccountNumberMasked,
    Currency Currency,
    bool IsActive,
    int TransactionCount,
    decimal Balance,
    DateTimeOffset? LastImport,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CreateAccountRequest(
    string Name,
    string Institution,
    string? AccountNumber,
    Currency Currency
);

public record UpdateAccountRequest(
    string Name,
    string Institution,
    bool IsActive
);
```

## React Components

### Account List
```typescript
// src/features/accounts/AccountList.tsx
export function AccountList({ profileId }: { profileId: string }) {
  const { data: accounts, isLoading } = useAccounts(profileId);
  
  if (isLoading) return <AccountListSkeleton />;
  
  return (
    <div className="space-y-4">
      {accounts?.map(account => (
        <AccountCard key={account.id} account={account} />
      ))}
      <AddAccountButton profileId={profileId} />
    </div>
  );
}
```

### Account Card
```typescript
// src/features/accounts/AccountCard.tsx
export function AccountCard({ account }: { account: AccountListDto }) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>{account.name}</CardTitle>
          <CardDescription>{account.institution}</CardDescription>
        </div>
        <Badge variant={account.isActive ? "default" : "secondary"}>
          {account.currency}
        </Badge>
      </CardHeader>
      <CardContent>
        <div className="flex justify-between text-sm text-muted-foreground">
          <span>{account.transactionCount} transactions</span>
          {account.lastImport && (
            <span>Last import: {formatDate(account.lastImport)}</span>
          )}
        </div>
      </CardContent>
      <CardFooter className="gap-2">
        <Button variant="outline" size="sm" asChild>
          <Link to={`/accounts/${account.id}/import`}>
            <Upload className="mr-2 h-4 w-4" />
            Import
          </Link>
        </Button>
        <Button variant="ghost" size="sm" asChild>
          <Link to={`/accounts/${account.id}`}>
            View
          </Link>
        </Button>
      </CardFooter>
    </Card>
  );
}
```

## Testing

```csharp
[Fact]
public async Task CreateAccount_WithValidData_ReturnsCreated()
{
    // Arrange
    await AuthenticateAsync();
    var profile = await CreateProfileAsync("Test");
    var request = new 
    { 
        name = "Checking", 
        institution = "Test Bank",
        accountNumber = "PT50123456789",
        currency = "EUR"
    };
    
    // Act
    var response = await Client.PostAsJsonAsync(
        $"/api/profiles/{profile.Id}/accounts", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var account = await response.Content.ReadFromJsonAsync<AccountDto>();
    account!.AccountNumberMasked.Should().Be("****6789");
}

[Fact]
public async Task GetAccounts_ForOtherUsersProfile_ReturnsNotFound()
{
    // Arrange
    var otherUserProfile = await CreateProfileForOtherUserAsync();
    await AuthenticateAsync();
    
    // Act
    var response = await Client.GetAsync(
        $"/api/profiles/{otherUserProfile.Id}/accounts");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

## Business Rules

1. Account names must be unique within a profile
2. Account numbers are stored masked (last 4 digits only)
3. Currency is immutable after creation (transactions depend on it)
4. Deleting an account cascades to all transactions and import sessions
5. Inactive accounts are hidden from import UI but data is preserved
6. Balance is calculated sum of all transactions (not stored)
