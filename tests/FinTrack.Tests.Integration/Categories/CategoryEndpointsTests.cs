using System.Net;
using System.Net.Http.Json;
using FinTrack.Core.Features.Categories;
using FinTrack.Core.Features.Profiles;
using FinTrack.Tests.Integration.Auth;

namespace FinTrack.Tests.Integration.Categories;

public class CategoryEndpointsTests : IClassFixture<FinTrackWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoryEndpointsTests(FinTrackWebApplicationFactory factory)
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
    public async Task CreateCategory_ReturnsCreated_WhenProfileExists()
    {
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var profile = await CreateTestProfile();

        var request = new CreateCategoryRequest("Groceries", "shopping-cart", "#22c55e", 0, null);
        var response = await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.Equal("Groceries", category!.Name);
        Assert.Equal("#22c55e", category.Color);
    }

    [Fact]
    public async Task GetCategories_ReturnsCategories_WhenExist()
    {
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var profile = await CreateTestProfile();

        await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/categories",
            new CreateCategoryRequest("Food", "folder", "#6B7280", 0, null));
        await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/categories",
            new CreateCategoryRequest("Transport", "folder", "#6B7280", 1, null));

        var response = await _client.GetAsync($"/api/profiles/{profile.Id}/categories");
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(categories!, c => c.Name == "Food");
        Assert.Contains(categories!, c => c.Name == "Transport");
    }

    [Fact]
    public async Task UpdateCategory_ReturnsUpdated_WhenExists()
    {
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var profile = await CreateTestProfile();

        var createResponse = await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/categories",
            new CreateCategoryRequest("Original", "folder", "#6B7280", 0, null));
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryDto>();

        var updateRequest = new UpdateCategoryRequest("Updated", "tag", "#f97316", 2, null);
        var response = await _client.PutAsJsonAsync(
            $"/api/profiles/{profile.Id}/categories/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.Equal("Updated", category!.Name);
        Assert.Equal("#f97316", category.Color);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNoContent_WhenExists()
    {
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var profile = await CreateTestProfile();

        var createResponse = await _client.PostAsJsonAsync($"/api/profiles/{profile.Id}/categories",
            new CreateCategoryRequest("Disposable", "folder", "#6B7280", 0, null));
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryDto>();

        var response = await _client.DeleteAsync($"/api/profiles/{profile.Id}/categories/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/profiles/{profile.Id}/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
