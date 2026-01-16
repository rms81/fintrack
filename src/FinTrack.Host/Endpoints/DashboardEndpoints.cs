using FinTrack.Core.Features.Dashboard;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

public static class DashboardEndpoints
{
    [WolverineGet("/api/profiles/{profileId}/dashboard/summary")]
    [Tags("Dashboard")]
    [EndpointSummary("Get dashboard summary")]
    [EndpointDescription("Returns aggregated financial summary including income, expenses, net balance, and comparison to previous period.")]
    [ProducesResponseType<DashboardSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetSummary(
        Guid profileId,
        [FromQuery] Guid? accountId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
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

        // Default to current month if no date range specified
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = fromDate ?? new DateOnly(now.Year, now.Month, 1);
        var to = toDate ?? now;

        var query = db.Transactions
            .Where(t => t.Account!.ProfileId == profileId)
            .Where(t => t.Date >= from && t.Date <= to);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        var transactions = await query
            .Select(t => new { t.Amount, t.CategoryId })
            .ToListAsync(ct);

        var totalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
        var totalExpenses = Math.Abs(transactions.Where(t => t.Amount < 0).Sum(t => t.Amount));
        var netBalance = totalIncome - totalExpenses;
        var transactionCount = transactions.Count;
        var uncategorizedCount = transactions.Count(t => t.CategoryId == null);

        // Get top spending category
        var topCategory = await query
            .Where(t => t.Amount < 0 && t.CategoryId != null)
            .GroupBy(t => new { t.CategoryId, t.Category!.Name })
            .Select(g => new { g.Key.CategoryId, g.Key.Name, Total = Math.Abs(g.Sum(t => t.Amount)) })
            .OrderByDescending(g => g.Total)
            .FirstOrDefaultAsync(ct);

        // Calculate previous period for comparison
        var periodDays = (to.ToDateTime(TimeOnly.MinValue) - from.ToDateTime(TimeOnly.MinValue)).Days + 1;
        var previousFrom = from.AddDays(-periodDays);
        var previousTo = from.AddDays(-1);

        var previousQuery = db.Transactions
            .Where(t => t.Account!.ProfileId == profileId)
            .Where(t => t.Date >= previousFrom && t.Date <= previousTo)
            .Where(t => t.Amount < 0);

        if (accountId.HasValue)
            previousQuery = previousQuery.Where(t => t.AccountId == accountId.Value);

        var previousExpenses = Math.Abs(await previousQuery.SumAsync(t => t.Amount, ct));

        decimal? expenseChangePercentage = previousExpenses > 0
            ? ((totalExpenses - previousExpenses) / previousExpenses) * 100
            : null;

        var result = new DashboardSummaryDto(
            totalIncome,
            totalExpenses,
            netBalance,
            transactionCount,
            uncategorizedCount,
            topCategory?.Name,
            topCategory?.Total,
            previousExpenses > 0 ? previousExpenses : null,
            expenseChangePercentage);

        return Results.Ok(result);
    }

    [WolverineGet("/api/profiles/{profileId}/dashboard/spending-by-category")]
    [Tags("Dashboard")]
    [EndpointSummary("Get spending breakdown by category")]
    [EndpointDescription("Returns spending aggregated by category with percentages. Only includes expenses (negative amounts).")]
    [ProducesResponseType<List<CategorySpendingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetSpendingByCategory(
        Guid profileId,
        [FromQuery] Guid? accountId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
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

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = fromDate ?? new DateOnly(now.Year, now.Month, 1);
        var to = toDate ?? now;

        var query = db.Transactions
            .Where(t => t.Account!.ProfileId == profileId)
            .Where(t => t.Date >= from && t.Date <= to)
            .Where(t => t.Amount < 0); // Only expenses

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        var categoryGroups = await query
            .GroupBy(t => new {
                CategoryId = t.CategoryId ?? Guid.Empty,
                CategoryName = t.Category != null ? t.Category.Name : "Uncategorized",
                CategoryColor = t.Category != null ? t.Category.Color : "#9CA3AF"
            })
            .Select(g => new {
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Key.CategoryColor,
                Amount = Math.Abs(g.Sum(t => t.Amount)),
                Count = g.Count()
            })
            .OrderByDescending(g => g.Amount)
            .ToListAsync(ct);

        var totalSpending = categoryGroups.Sum(g => g.Amount);

        var result = categoryGroups.Select(g => new CategorySpendingDto(
            g.CategoryId,
            g.CategoryName,
            g.CategoryColor,
            g.Amount,
            totalSpending > 0 ? (g.Amount / totalSpending) * 100 : 0,
            g.Count
        )).ToList();

        return Results.Ok(result);
    }

    [WolverineGet("/api/profiles/{profileId}/dashboard/spending-over-time")]
    [Tags("Dashboard")]
    [EndpointSummary("Get spending over time")]
    [EndpointDescription("Returns income and expenses aggregated by time period (day, week, or month). For weekly grouping, weekStartDay parameter controls the first day of the week (0=Sunday, 1=Monday, etc.). Defaults to Sunday (0).")]
    [ProducesResponseType<List<SpendingOverTimeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetSpendingOverTime(
        Guid profileId,
        [FromQuery] Guid? accountId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        FinTrackDbContext db = null!,
        ICurrentUser currentUser = null!,
        [FromQuery] string granularity = "month", // day, week, month
        [FromQuery] int weekStartDay = 0, // 0=Sunday, 1=Monday, ..., 6=Saturday
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profileExists = await db.Profiles
            .AnyAsync(p => p.Id == profileId && p.UserId == userId, ct);

        if (!profileExists)
            return Results.NotFound();

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = fromDate ?? new DateOnly(now.Year - 1, now.Month, 1); // Default to last 12 months
        var to = toDate ?? now;

        var query = db.Transactions
            .Where(t => t.Account!.ProfileId == profileId)
            .Where(t => t.Date >= from && t.Date <= to);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        var transactions = await query
            .Select(t => new { t.Date, t.Amount })
            .ToListAsync(ct);

        // Validate and clamp weekStartDay to valid range (0-6)
        weekStartDay = Math.Clamp(weekStartDay, 0, 6);

        // Group by granularity
        var grouped = granularity.ToLower() switch
        {
            "day" => transactions.GroupBy(t => t.Date),
            "week" => transactions.GroupBy(t => {
                // Calculate days from the configured week start day
                var currentDayOfWeek = (int)t.Date.DayOfWeek;
                var diff = (currentDayOfWeek - weekStartDay + 7) % 7;
                return t.Date.AddDays(-diff);
            }),
            _ => transactions.GroupBy(t => new DateOnly(t.Date.Year, t.Date.Month, 1))
        };

        var result = grouped
            .Select(g => new SpendingOverTimeDto(
                g.Key,
                g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                Math.Abs(g.Where(t => t.Amount < 0).Sum(t => t.Amount)),
                g.Sum(t => t.Amount)
            ))
            .OrderBy(d => d.Date)
            .ToList();

        // Fill gaps for monthly granularity
        if (granularity.ToLower() == "month")
        {
            var filledResult = new List<SpendingOverTimeDto>();
            var current = new DateOnly(from.Year, from.Month, 1);
            var end = new DateOnly(to.Year, to.Month, 1);

            while (current <= end)
            {
                var existing = result.FirstOrDefault(r => r.Date == current);
                filledResult.Add(existing ?? new SpendingOverTimeDto(current, 0, 0, 0));
                current = current.AddMonths(1);
            }
            result = filledResult;
        }

        return Results.Ok(result);
    }

    [WolverineGet("/api/profiles/{profileId}/dashboard/top-merchants")]
    [Tags("Dashboard")]
    [EndpointSummary("Get top merchants by spending")]
    [EndpointDescription("Returns the top merchants (by transaction description) ranked by total spending.")]
    [ProducesResponseType<List<TopMerchantDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetTopMerchants(
        Guid profileId,
        [FromQuery] Guid? accountId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        FinTrackDbContext db = null!,
        ICurrentUser currentUser = null!,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profileExists = await db.Profiles
            .AnyAsync(p => p.Id == profileId && p.UserId == userId, ct);

        if (!profileExists)
            return Results.NotFound();

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = fromDate ?? new DateOnly(now.Year, now.Month, 1);
        var to = toDate ?? now;

        limit = Math.Clamp(limit, 1, 50);

        var query = db.Transactions
            .Where(t => t.Account!.ProfileId == profileId)
            .Where(t => t.Date >= from && t.Date <= to)
            .Where(t => t.Amount < 0); // Only expenses

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        // Group by normalized description (uppercase, trimmed)
        var merchants = await query
            .GroupBy(t => t.Description.ToUpper().Trim())
            .Select(g => new {
                Merchant = g.Key,
                TotalAmount = Math.Abs(g.Sum(t => t.Amount)),
                TransactionCount = g.Count(),
                LastTransactionDate = g.Max(t => t.Date),
                // Get most common category name
                MostCommonCategory = g
                    .Where(t => t.CategoryId != null)
                    .GroupBy(t => t.Category!.Name)
                    .OrderByDescending(cg => cg.Count())
                    .Select(cg => cg.Key)
                    .FirstOrDefault()
            })
            .OrderByDescending(m => m.TotalAmount)
            .Take(limit)
            .ToListAsync(ct);

        var result = merchants.Select(m => new TopMerchantDto(
            m.Merchant,
            m.TotalAmount,
            m.TransactionCount,
            m.LastTransactionDate,
            m.MostCommonCategory
        )).ToList();

        return Results.Ok(result);
    }
}
