# Authentication Module

## Overview
Cookie-based authentication using ASP.NET Core Identity, integrated directly into FinTrack.Host.

## User Stories

| ID | As a... | I want to... | So that... |
|----|---------|--------------|------------|
| US-AU1 | Visitor | register for an account with email and password | I can start tracking my expenses |
| US-AU2 | Visitor | log in with my credentials | I can access my financial data |
| US-AU3 | User | stay logged in across browser sessions | I don't have to log in every time |
| US-AU4 | User | log out of my account | my data is secure on shared devices |
| US-AU5 | User | see my account information | I can verify I'm logged into the correct account |
| US-AU6 | User | have my session automatically extended | I'm not logged out while actively using the app |

## Architecture Decision
Chose cookie-based auth over JWT for simplicity:
- No token refresh logic needed
- HttpOnly cookies prevent XSS attacks
- Works seamlessly with BFF pattern
- Session management built-in

## Dependencies

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.*" />
```

## Database Tables

ASP.NET Identity creates these tables automatically:
- `asp_net_users` - User accounts
- `asp_net_roles` - Roles (if needed)
- `asp_net_user_claims` - User claims
- `asp_net_user_logins` - External logins (if needed)
- `asp_net_user_tokens` - Tokens

## Configuration

### Program.cs Setup
```csharp
// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<FinTrackDbContext>()
.AddDefaultTokenProviders();

// Configure cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});
```

### ApplicationUser Entity
```csharp
public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    
    // Navigation
    public ICollection<Profile> Profiles { get; set; } = [];
}
```

## Endpoints

### POST /api/auth/register
Create new user account.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "displayName": "John Doe"
}
```

**Response:** `201 Created`
```json
{
  "id": "0193a1b2-...",
  "email": "user@example.com",
  "displayName": "John Doe"
}
```

### POST /api/auth/login
Authenticate and set cookie.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "rememberMe": true
}
```

**Response:** `200 OK` (sets HttpOnly cookie)
```json
{
  "id": "0193a1b2-...",
  "email": "user@example.com",
  "displayName": "John Doe"
}
```

### POST /api/auth/logout
Clear authentication cookie.

**Response:** `204 No Content`

### GET /api/auth/me
Get current user info.

**Response:** `200 OK`
```json
{
  "id": "0193a1b2-...",
  "email": "user@example.com",
  "displayName": "John Doe"
}
```

## Wolverine Handlers

### RegisterUser Command
```csharp
public record RegisterUser(string Email, string Password, string DisplayName);

public record UserRegistered(Guid UserId, string Email, string DisplayName);

public static class RegisterUserHandler
{
    public static async Task<UserRegistered> HandleAsync(
        RegisterUser command,
        UserManager<ApplicationUser> userManager)
    {
        var user = new ApplicationUser
        {
            Email = command.Email,
            UserName = command.Email,
            DisplayName = command.DisplayName
        };
        
        var result = await userManager.CreateAsync(user, command.Password);
        
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => e.Description));
        }
        
        return new UserRegistered(user.Id, user.Email!, user.DisplayName);
    }
}
```

### LoginUser Command
```csharp
public record LoginUser(string Email, string Password, bool RememberMe);

public record UserLoggedIn(Guid UserId, string Email, string DisplayName);

public static class LoginUserHandler
{
    public static async Task<UserLoggedIn> HandleAsync(
        LoginUser command,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        var result = await signInManager.PasswordSignInAsync(
            command.Email, 
            command.Password, 
            command.RememberMe, 
            lockoutOnFailure: true);
            
        if (!result.Succeeded)
        {
            throw new UnauthorizedException("Invalid credentials");
        }
        
        var user = await userManager.FindByEmailAsync(command.Email);
        return new UserLoggedIn(user!.Id, user.Email!, user.DisplayName);
    }
}
```

## React Integration

### Auth Context
```typescript
// src/features/auth/AuthContext.tsx
interface AuthState {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const { data: user, isLoading } = useQuery({
    queryKey: ['auth', 'me'],
    queryFn: () => api.get('/api/auth/me'),
    retry: false,
  });
  
  // ...
}
```

### Protected Routes
```typescript
// src/features/auth/ProtectedRoute.tsx
export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  
  if (isLoading) return <LoadingSpinner />;
  if (!isAuthenticated) return <Navigate to="/login" />;
  
  return children;
}
```

## Testing

### Integration Test
```csharp
[Fact]
public async Task Register_WithValidData_CreatesUser()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new { email = "test@example.com", password = "Test123!", displayName = "Test" };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/auth/register", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

## Security Considerations

- Passwords hashed with ASP.NET Identity (PBKDF2)
- HttpOnly cookies prevent JavaScript access
- SameSite=Strict prevents CSRF
- Account lockout after failed attempts
- Email confirmation (optional, Phase 5)
