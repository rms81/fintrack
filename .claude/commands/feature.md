# Create Feature Slice

Create a new vertical feature slice for the FinTrack application.

## Arguments
- `$ARGUMENTS` - The feature name (e.g., "CreateProfile", "ImportTransactions")

## Instructions

When the user runs `/feature <FeatureName>`, create a complete vertical slice including:

1. **Command/Query** in `src/FinTrack.Core/Features/{FeatureArea}/`:
   - If the name starts with "Get" or "List", create a Query record
   - Otherwise, create a Command record
   - Include request validation using data annotations or FluentValidation

2. **Handler** in the same directory:
   - Use Wolverine handler conventions (static Handle method or instance method)
   - Inject dependencies via method parameters
   - Return appropriate response type

3. **DTOs** if needed:
   - Request DTO (if complex input)
   - Response DTO

4. **Endpoint** in `src/FinTrack.Host/Endpoints/`:
   - Use Minimal API style
   - Follow REST conventions
   - Include OpenAPI metadata

5. **Integration Test** in `tests/FinTrack.Tests.Integration/`:
   - Test happy path
   - Test validation errors
   - Test authorization

## Example Output Structure

For `/feature CreateProfile`:

```
src/FinTrack.Core/Features/Profiles/
├── CreateProfile.cs          # Command + Handler
└── ProfileDto.cs             # DTO (if not exists)

src/FinTrack.Host/Endpoints/
└── ProfileEndpoints.cs       # Add new endpoint

tests/FinTrack.Tests.Integration/
└── ProfileEndpointsTests.cs  # Add new test
```

## Code Templates

### Command with Handler (Wolverine style)
```csharp
namespace FinTrack.Core.Features.Profiles;

public record CreateProfile(
    Guid UserId,
    string Name,
    ProfileType Type
);

public static class CreateProfileHandler
{
    public static async Task<ProfileDto> Handle(
        CreateProfile command,
        FinTrackDbContext db,
        CancellationToken ct)
    {
        var profile = new Profile
        {
            Id = Guid.CreateVersion7(),
            UserId = command.UserId,
            Name = command.Name,
            Type = command.Type,
            CreatedAt = DateTime.UtcNow
        };
        
        db.Profiles.Add(profile);
        await db.SaveChangesAsync(ct);
        
        return profile.ToDto();
    }
}
```

### Query with Handler
```csharp
namespace FinTrack.Core.Features.Profiles;

public record GetProfiles(Guid UserId);

public static class GetProfilesHandler
{
    public static async Task<IReadOnlyList<ProfileDto>> Handle(
        GetProfiles query,
        FinTrackDbContext db,
        CancellationToken ct)
    {
        return await db.Profiles
            .Where(p => p.UserId == query.UserId)
            .Select(p => p.ToDto())
            .ToListAsync(ct);
    }
}
```

### Minimal API Endpoint
```csharp
public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profiles")
            .WithTags("Profiles")
            .RequireAuthorization();
        
        group.MapPost("/", CreateProfile)
            .WithName("CreateProfile")
            .WithSummary("Create a new profile");
    }
    
    private static async Task<IResult> CreateProfile(
        CreateProfileRequest request,
        IMessageBus bus,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var command = new CreateProfile(user.GetUserId(), request.Name, request.Type);
        var result = await bus.InvokeAsync<ProfileDto>(command, ct);
        return TypedResults.Created($"/api/profiles/{result.Id}", result);
    }
}
```

Now create the feature slice for: $ARGUMENTS
