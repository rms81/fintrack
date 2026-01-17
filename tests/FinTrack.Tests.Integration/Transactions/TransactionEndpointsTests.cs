using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Features.Accounts;
using FinTrack.Core.Features.Categories;
using FinTrack.Core.Features.Profiles;
using FinTrack.Core.Features.Transactions;
using FinTrack.Infrastructure.Persistence;
using FinTrack.Tests.Integration.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Tests.Integration.Transactions;

public class TransactionEndpointsTests : IClassFixture<FinTrackWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FinTrackWebApplicationFactory _factory;

    public TransactionEndpointsTests(FinTrackWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Reset();
    }

    private async Task<ProfileDto> CreateTestProfile()
    {
        var request = new CreateProfileRequest("Test Profile");
        var response = await _client.PostAsJsonAsync("/api/profiles", request);
        return (await response.Content.ReadFromJsonAsync<ProfileDto>())!;
    }

    private async Task<AccountDto> CreateTestAccount(Guid profileId)
    {
        var request = new CreateAccountRequest("Checking", "Bank", "USD");
        var response = await _client.PostAsJsonAsync($"/api/profiles/{profileId}/accounts", request);
        return (await response.Content.ReadFromJsonAsync<AccountDto>())!;
    }

    private async Task<CategoryDto> CreateTestCategory(Guid profileId)
    {
        var request = new CreateCategoryRequest("Groceries", "shopping-cart", "#22c55e", 0, null);
        var response = await _client.PostAsJsonAsync($"/api/profiles/{profileId}/categories", request);
        return (await response.Content.ReadFromJsonAsync<CategoryDto>())!;
    }

    [Fact]
    public async Task GetTransactions_ReturnsPagedResults()
    {
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var profile = await CreateTestProfile();
        var account = await CreateTestAccount(profile.Id);

        // Create a few transactions via import confirm
        await SeedTransactions(account.Id, 3);

        var response = await _client.GetAsync($"/api/profiles/{profile.Id}/transactions?page=1&pageSize=2");
        var page = await response.Content.ReadFromJsonAsync<TransactionPage>();

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Unexpected status {response.StatusCode}: {errorBody}");
        }

        Assert.Equal(2, page!.Items.Count);
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task UpdateTransaction_UpdatesCategoryNotesTags()
    {
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var profile = await CreateTestProfile();
        var account = await CreateTestAccount(profile.Id);
        var category = await CreateTestCategory(profile.Id);

        await SeedTransactions(account.Id, 1);

        var pageResponse = await _client.GetAsync($"/api/profiles/{profile.Id}/transactions?page=1&pageSize=1");
        var page = await pageResponse.Content.ReadFromJsonAsync<TransactionPage>();
        var transaction = page?.Items?.FirstOrDefault();
        Assert.NotNull(transaction);

        var request = new UpdateTransactionRequest(category.Id, "Grocery run", new[] { "food", "weekly" });
        var updateResponse = await _client.PutAsJsonAsync($"/api/transactions/{transaction!.Id}", request);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TransactionDto>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(category.Id, updated!.CategoryId);
        Assert.Equal("Grocery run", updated.Notes);
        Assert.Contains("food", updated.Tags);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsNoContent_WhenExists()
    {
        TestAuthHandler.TestUserId = Guid.NewGuid().ToString();
        var profile = await CreateTestProfile();
        var account = await CreateTestAccount(profile.Id);

        await SeedTransactions(account.Id, 1);

        var pageResponse = await _client.GetAsync($"/api/profiles/{profile.Id}/transactions?page=1&pageSize=1");
        var page = await pageResponse.Content.ReadFromJsonAsync<TransactionPage>();
        var transaction = page?.Items?.FirstOrDefault();
        Assert.NotNull(transaction);

        var deleteResponse = await _client.DeleteAsync($"/api/transactions/{transaction!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    private async Task SeedTransactions(Guid accountId, int rows)
    {
        var now = DateTime.UtcNow;
        var transactions = new List<Transaction>();

        for (var i = 0; i < rows; i++)
        {
            transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Date = DateOnly.FromDateTime(now.AddDays(-i)),
                Amount = -10.00m,
                Description = $"Test {i + 1}",
                DuplicateHash = Guid.NewGuid().ToString("N")
            });
        }

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FinTrackDbContext>();

        var accountExists = await db.Accounts.AnyAsync(a => a.Id == accountId);
        if (!accountExists)
        {
            throw new InvalidOperationException($"Account {accountId} not found for seeding");
        }

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();
    }
}
