using System.Net;
using System.Net.Http.Json;
using FinTrack.Core.Domain.Enums;
using FinTrack.Core.Features.Profiles;
using FinTrack.Tests.Integration.Auth;

namespace FinTrack.Tests.Integration.Profiles;

public class ProfileEndpointsTests : IClassFixture<FinTrackWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProfileEndpointsTests(FinTrackWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        TestAuthHandler.Reset();
    }

    [Fact]
    public async Task CreateProfile_ReturnsCreated_WhenAuthenticated()
    {
        // Arrange
        var request = new CreateProfileRequest("Test Profile", ProfileType.Personal);

        // Act
        var response = await _client.PostAsJsonAsync("/api/profiles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("Test Profile", profile.Name);
        Assert.Equal(ProfileType.Personal, profile.Type);
    }

    [Fact]
    public async Task CreateProfile_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        TestAuthHandler.IsAuthenticated = false;
        var request = new CreateProfileRequest("Test Profile");

        // Act
        var response = await _client.PostAsJsonAsync("/api/profiles", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfiles_ReturnsEmptyList_WhenNoProfiles()
    {
        // Arrange
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync("/api/profiles");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profiles = await response.Content.ReadFromJsonAsync<List<ProfileDto>>();
        Assert.NotNull(profiles);
        Assert.Empty(profiles);
    }

    [Fact]
    public async Task GetProfiles_ReturnsUserProfiles_WhenProfilesExist()
    {
        // Arrange
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var request = new CreateProfileRequest("My Profile", ProfileType.Business);
        await _client.PostAsJsonAsync("/api/profiles", request);

        // Act
        var response = await _client.GetAsync("/api/profiles");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profiles = await response.Content.ReadFromJsonAsync<List<ProfileDto>>();
        Assert.NotNull(profiles);
        Assert.Single(profiles);
        Assert.Equal("My Profile", profiles[0].Name);
    }

    [Fact]
    public async Task GetProfile_ReturnsProfile_WhenExists()
    {
        // Arrange
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var createRequest = new CreateProfileRequest("Specific Profile");
        var createResponse = await _client.PostAsJsonAsync("/api/profiles", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProfileDto>();

        // Act
        var response = await _client.GetAsync($"/api/profiles/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal(created.Id, profile.Id);
    }

    [Fact]
    public async Task GetProfile_ReturnsNotFound_WhenNotExists()
    {
        // Act
        var response = await _client.GetAsync($"/api/profiles/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_ReturnsUpdated_WhenExists()
    {
        // Arrange
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var createRequest = new CreateProfileRequest("Original Name");
        var createResponse = await _client.PostAsJsonAsync("/api/profiles", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProfileDto>();

        var updateRequest = new UpdateProfileRequest("Updated Name", ProfileType.Business);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/profiles/{created!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("Updated Name", profile.Name);
        Assert.Equal(ProfileType.Business, profile.Type);
    }

    [Fact]
    public async Task DeleteProfile_ReturnsNoContent_WhenExists()
    {
        // Arrange
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var createRequest = new CreateProfileRequest("To Delete");
        var createResponse = await _client.PostAsJsonAsync("/api/profiles", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProfileDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/profiles/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deleted
        var getResponse = await _client.GetAsync($"/api/profiles/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProfile_ReturnsNotFound_WhenNotExists()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/profiles/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
