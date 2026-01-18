using System.Globalization;
using FinTrack.Core.Features.Transactions;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Caching;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

/// <summary>
/// Endpoints for managing financial transactions.
/// </summary>
public static class TransactionEndpoints
{
    [WolverineGet("/api/profiles/{profileId}/transactions")]
    [Tags("Transactions")]
    [EndpointSummary("List transactions with filtering")]
    [EndpointDescription("Returns a paginated list of transactions for a profile. Supports filtering by account, category, date range, amount range, search text, and uncategorized status.")]
    [ProducesResponseType<TransactionPage>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetTransactions(
        Guid profileId,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? categoryId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery(Name = "minAmount")] string? minAmount,
        [FromQuery(Name = "maxAmount")] string? maxAmount,
        [FromQuery] string? search,
        [FromQuery] bool? uncategorized,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        FinTrackDbContext db = null!,
        ICurrentUser currentUser = null!,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profileExists = await db.Profiles
            .AnyAsync(p => p.Id == profileId && p.UserId == userId, ct);

        if (!profileExists)
            return Results.NotFound();

        var query = db.Transactions
            .Where(t => t.Account!.ProfileId == profileId);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (fromDate.HasValue)
            query = query.Where(t => t.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.Date <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(minAmount) && decimal.TryParse(minAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var minAmountValue))
            query = query.Where(t => t.Amount >= minAmountValue);

        if (!string.IsNullOrWhiteSpace(maxAmount) && decimal.TryParse(maxAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var maxAmountValue))
            query = query.Where(t => t.Amount <= maxAmountValue);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => EF.Functions.ILike(t.Description, $"%{search}%"));

        if (uncategorized == true)
            query = query.Where(t => t.CategoryId == null);

        var totalCount = await query.CountAsync(ct);

        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var transactions = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.AccountId,
                t.CategoryId,
                t.Category != null ? t.Category.Name : null,
                t.Date,
                t.Amount,
                t.Description,
                t.Notes,
                t.Tags,
                t.CreatedAt,
                t.UpdatedAt))
            .ToListAsync(ct);

        var result = new TransactionPage(transactions, totalCount, page, pageSize, totalPages);
        return Results.Ok(result);
    }

    [WolverineGet("/api/transactions/{id}")]
    [Tags("Transactions")]
    [EndpointSummary("Get a transaction by ID")]
    [EndpointDescription("Returns a single transaction by its unique identifier.")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetTransaction(
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var transaction = await db.Transactions
            .Where(t => t.Id == id && t.Account!.Profile!.UserId == userId)
            .Select(t => new TransactionDto(
                t.Id,
                t.AccountId,
                t.CategoryId,
                t.Category != null ? t.Category.Name : null,
                t.Date,
                t.Amount,
                t.Description,
                t.Notes,
                t.Tags,
                t.CreatedAt,
                t.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return transaction is null ? Results.NotFound() : Results.Ok(transaction);
    }

    [WolverinePut("/api/transactions/{id}")]
    [Tags("Transactions")]
    [EndpointSummary("Update a transaction")]
    [EndpointDescription("Updates a transaction's category, notes, and tags. The date, amount, and description are read-only as they come from the import source.")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> UpdateTransaction(
        Guid id,
        [FromBody] UpdateTransactionRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        ICacheService cache,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var transaction = await db.Transactions
            .Include(t => t.Account)
                .ThenInclude(a => a!.Profile)
            .Where(t => t.Id == id && t.Account!.Profile!.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (transaction is null)
            return Results.NotFound();

        var profileId = transaction.Account!.ProfileId;

        // Verify category belongs to same profile if specified
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await db.Categories
                .AnyAsync(c => c.Id == request.CategoryId.Value && c.ProfileId == profileId, ct);
            if (!categoryExists)
                return Results.BadRequest(new { error = "Category not found in this profile" });
        }

        transaction.CategoryId = request.CategoryId;
        transaction.Notes = request.Notes ?? transaction.Notes;
        transaction.Tags = request.Tags ?? transaction.Tags;

        await db.SaveChangesAsync(ct);

        // Invalidate dashboard caches (transaction amounts/categories affect dashboard)
        foreach (var prefix in CacheKeys.DashboardPrefixes(profileId))
        {
            cache.RemoveByPrefix(prefix);
        }
        // Also invalidate categories cache (transaction count changes)
        cache.Remove(CacheKeys.Categories(profileId));

        var categoryName = request.CategoryId.HasValue
            ? await db.Categories.Where(c => c.Id == request.CategoryId.Value).Select(c => c.Name).FirstOrDefaultAsync(ct)
            : null;

        var result = new TransactionDto(
            transaction.Id,
            transaction.AccountId,
            transaction.CategoryId,
            categoryName,
            transaction.Date,
            transaction.Amount,
            transaction.Description,
            transaction.Notes,
            transaction.Tags,
            transaction.CreatedAt,
            transaction.UpdatedAt);

        return Results.Ok(result);
    }

    [WolverineDelete("/api/transactions/{id}")]
    [Tags("Transactions")]
    [EndpointSummary("Delete a transaction")]
    [EndpointDescription("Permanently deletes a transaction.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> DeleteTransaction(
        Guid id,
        [FromServices] FinTrackDbContext db,
        [FromServices] ICurrentUser currentUser,
        [FromServices] ICacheService cache,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var transaction = await db.Transactions
            .Include(t => t.Account)
            .Where(t => t.Id == id && t.Account!.Profile!.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (transaction is null)
            return Results.NotFound();

        var profileId = transaction.Account!.ProfileId;

        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync(ct);

        // Invalidate dashboard and categories caches
        foreach (var prefix in CacheKeys.DashboardPrefixes(profileId))
        {
            cache.RemoveByPrefix(prefix);
        }
        cache.Remove(CacheKeys.Categories(profileId));

        return Results.NoContent();
    }
}
