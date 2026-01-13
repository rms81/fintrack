using System.Net;
using System.Net.Http.Json;
using FinTrack.Core.Features.Accounts;
using FinTrack.Core.Features.Profiles;
using FinTrack.Tests.Integration.Auth;

namespace FinTrack.Tests.Integration.Accounts;

public class AccountEndpointsTests : IClassFixture<FinTrackWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountEndpointsTests(FinTrackWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        TestAuthHandler.Reset();
    }

    private async Task<ProfileDto> CreateTestProfile()
    {
        var request = new CreateProfileRequest("Test Profile");
        var response = await _client.PostAsJsonAsync("/api/profiles", request);
        return (await response.Content.ReadFromJsonAsync<ProfileDto>())!;
    }

    [Fact]
    public async Task CreateAccount_ReturnsCreated_WhenProfileExists()
    {
        // Arrange
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var profile = await CreateTestProfile();
        var request = new CreateAccountRequest("Checking Account", "Chase Bank", "USD");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/accounts", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountDto>();
        Assert.NotNull(account);
        Assert.Equal("Checking Account", account.Name);
        Assert.Equal("Chase Bank", account.BankName);
        Assert.Equal("USD", account.Currency);
    }

    [Fact]
    public async Task CreateAccount_ReturnsNotFound_WhenProfileNotExists()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/profiles/{Guid.NewGuid()}/accounts", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_DefaultsToEUR_WhenCurrencyNotSpecified()
    {
        // Arrange
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var profile = await CreateTestProfile();
        var request = new CreateAccountRequest("Savings");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/accounts", request);

        // Assert
        var account = await response.Content.ReadFromJsonAsync<AccountDto>();
        Assert.Equal("EUR", account!.Currency);
    }

    [Fact]
    public async Task GetAccounts_ReturnsEmpty_WhenNoAccounts()
    {
        // Arrange
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var profile = await CreateTestProfile();

        // Act
        var response = await _client.GetAsync($"/api/profiles/{profile.Id}/accounts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountDto>>();
        Assert.NotNull(accounts);
        Assert.Empty(accounts);
    }

    [Fact]
    public async Task GetAccounts_ReturnsAccounts_WhenExist()
    {
        // Arrange
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var profile = await CreateTestProfile();
        await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/accounts",
            new CreateAccountRequest("Account 1"));
        await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/accounts",
            new CreateAccountRequest("Account 2"));

        // Act
        var response = await _client.GetAsync($"/api/profiles/{profile.Id}/accounts");

        // Assert
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountDto>>();
        Assert.Equal(2, accounts!.Count);
    }

    [Fact]
    public async Task GetAccount_ReturnsAccount_WhenExists()
    {
        // Arrange
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var profile = await CreateTestProfile();
        var createResponse = await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/accounts",
            new CreateAccountRequest("My Account"));
        var created = await createResponse.Content.ReadFromJsonAsync<AccountDto>();

        // Act
        var response = await _client.GetAsync($"/api/profiles/{profile.Id}/accounts/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountDto>();
        Assert.Equal(created.Id, account!.Id);
    }

    [Fact]
    public async Task GetAccount_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var profile = await CreateTestProfile();

        // Act
        var response = await _client.GetAsync($"/api/profiles/{profile.Id}/accounts/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccount_ReturnsUpdated_WhenExists()
    {
        // Arrange
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var profile = await CreateTestProfile();
        var createResponse = await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/accounts",
            new CreateAccountRequest("Original"));
        var created = await createResponse.Content.ReadFromJsonAsync<AccountDto>();

        var updateRequest = new UpdateAccountRequest("Updated", "New Bank", "GBP");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/profiles/{profile.Id}/accounts/{created!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountDto>();
        Assert.Equal("Updated", account!.Name);
        Assert.Equal("New Bank", account.BankName);
        Assert.Equal("GBP", account.Currency);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsNoContent_WhenExists()
    {
        // Arrange
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var profile = await CreateTestProfile();
        var createResponse = await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/accounts",
            new CreateAccountRequest("To Delete"));
        var created = await createResponse.Content.ReadFromJsonAsync<AccountDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/profiles/{profile.Id}/accounts/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deleted
        var getResponse = await _client.GetAsync($"/api/profiles/{profile.Id}/accounts/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Account_NotAccessible_FromOtherUsersProfile()
    {
        // Arrange - Create account with user 1
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var profile = await CreateTestProfile();
        var createResponse = await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/accounts",
            new CreateAccountRequest("Private Account"));
        var created = await createResponse.Content.ReadFromJsonAsync<AccountDto>();

        // Act - Try to access with user 2
        TestAuthHandler.TestUserId = $"user-{Guid.NewGuid()}";
        var response = await _client.GetAsync($"/api/profiles/{profile.Id}/accounts/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
