# Create Integration Test

Create integration tests for an API endpoint in the FinTrack application.

## Arguments
- `$ARGUMENTS` - The endpoint or feature to test (e.g., "POST /api/profiles", "TransactionImport")

## Instructions

When the user runs `/test <endpoint>`, create integration tests that:

1. Test the happy path
2. Test validation errors
3. Test authorization (if applicable)
4. Test edge cases

## Test Infrastructure

The project uses:
- **xUnit** as the test framework
- **WebApplicationFactory** for in-process testing
- **Testcontainers** or real PostgreSQL for database
- **FluentAssertions** for assertions
- **Bogus** for test data generation

## Code Templates

### Test Class Structure
```csharp
// tests/FinTrack.Tests.Integration/Profiles/CreateProfileTests.cs
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FinTrack.Core.Domain.Entities;
using FinTrack.Host.Endpoints;

namespace FinTrack.Tests.Integration.Profiles;

public class CreateProfileTests : IntegrationTestBase
{
    public CreateProfileTests(FinTrackWebApplicationFactory factory) 
        : base(factory) { }

    [Fact]
    public async Task CreateProfile_WithValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateProfileRequest("My Business", ProfileType.Business);

        // Act
        var response = await Client.PostAsJsonAsync("/api/profiles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        profile.Should().NotBeNull();
        profile!.Name.Should().Be("My Business");
        profile.Type.Should().Be(ProfileType.Business);
        
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(profile.Id.ToString());
    }

    [Fact]
    public async Task CreateProfile_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateProfileRequest("", ProfileType.Personal);

        // Act
        var response = await Client.PostAsJsonAsync("/api/profiles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task CreateProfile_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange (no authentication)
        var request = new CreateProfileRequest("Test", ProfileType.Personal);

        // Act
        var response = await Client.PostAsJsonAsync("/api/profiles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProfile_WithDuplicateName_Succeeds()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateProfileRequest("Duplicate Name", ProfileType.Personal);
        
        // Create first profile
        await Client.PostAsJsonAsync("/api/profiles", request);

        // Act - Create second profile with same name
        var response = await Client.PostAsJsonAsync("/api/profiles", request);

        // Assert - Duplicate names are allowed
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### Base Test Class
```csharp
// tests/FinTrack.Tests.Integration/IntegrationTestBase.cs
using Microsoft.Extensions.DependencyInjection;
using FinTrack.Infrastructure.Persistence;

namespace FinTrack.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<FinTrackWebApplicationFactory>, IAsyncLifetime
{
    protected readonly FinTrackWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected Guid TestUserId { get; private set; }

    protected IntegrationTestBase(FinTrackWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Reset database state
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinTrackDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task AuthenticateAsync(string? email = null)
    {
        TestUserId = Guid.CreateVersion7();
        email ??= $"test-{TestUserId}@example.com";
        
        // Set authentication header (mock JWT or cookie)
        Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Test", $"UserId={TestUserId};Email={email}");
    }

    protected async Task<T> CreateTestEntity<T>(T entity) where T : class
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinTrackDbContext>();
        db.Set<T>().Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    protected async Task<Guid> CreateTestProfile(string name = "Test Profile")
    {
        var profile = new Profile
        {
            Id = Guid.CreateVersion7(),
            UserId = TestUserId,
            Name = name,
            Type = ProfileType.Personal,
            CreatedAt = DateTime.UtcNow
        };
        await CreateTestEntity(profile);
        return profile.Id;
    }
}
```

### Web Application Factory
```csharp
// tests/FinTrack.Tests.Integration/FinTrackWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using FinTrack.Infrastructure.Persistence;

namespace FinTrack.Tests.Integration;

public class FinTrackWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FinTrackDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test DbContext
            services.AddDbContext<FinTrackDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            // Mock LLM service
            services.AddSingleton<ILlmService, MockLlmService>();

            // Add test authentication
            services.AddAuthentication("Test")
                .AddScheme<TestAuthOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
```

### Test Data Builder
```csharp
// tests/FinTrack.Tests.Integration/TestDataBuilders/TransactionBuilder.cs
using Bogus;
using FinTrack.Core.Domain.Entities;

namespace FinTrack.Tests.Integration.TestDataBuilders;

public class TransactionBuilder
{
    private readonly Faker _faker = new();
    private Guid _accountId;
    private DateOnly? _date;
    private string? _description;
    private decimal? _amount;
    private Guid? _categoryId;

    public TransactionBuilder WithAccountId(Guid accountId)
    {
        _accountId = accountId;
        return this;
    }

    public TransactionBuilder WithDate(DateOnly date)
    {
        _date = date;
        return this;
    }

    public TransactionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TransactionBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public TransactionBuilder WithCategory(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public Transaction Build()
    {
        var description = _description ?? _faker.Commerce.ProductName();
        
        return new Transaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = _accountId,
            Date = _date ?? DateOnly.FromDateTime(_faker.Date.Recent(30)),
            Description = description,
            NormalizedDescription = description.ToUpperInvariant(),
            Amount = _amount ?? _faker.Finance.Amount(-500, 500),
            CategoryId = _categoryId,
            Tags = [],
            CreatedAt = DateTime.UtcNow
        };
    }

    public List<Transaction> BuildMany(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => Build())
            .ToList();
    }
}
```

Now create the integration tests for: $ARGUMENTS
