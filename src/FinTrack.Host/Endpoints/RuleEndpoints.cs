using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Features.Rules;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

public static class RuleEndpoints
{
    [WolverinePost("/api/profiles/{profileId}/rules")]
    public static async Task<IResult> CreateRule(
        Guid profileId,
        [FromBody] CreateRuleRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        IRulesEngine rulesEngine,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var profile = await db.Profiles
            .Where(p => p.Id == profileId && p.User!.ExternalId == currentUser.Id)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return Results.NotFound();

        var validation = rulesEngine.ValidateRule(request.RuleToml);
        if (!validation.IsValid)
            return Results.BadRequest(new { error = validation.ErrorMessage });

        var rule = new CategorizationRule
        {
            ProfileId = profileId,
            Name = request.Name,
            Priority = request.Priority,
            RuleToml = request.RuleToml,
            IsActive = request.IsActive
        };

        db.CategorizationRules.Add(rule);
        await db.SaveChangesAsync(ct);

        var result = new RuleDto(
            rule.Id,
            rule.Name,
            rule.Priority,
            rule.RuleToml,
            rule.IsActive,
            rule.CreatedAt,
            rule.UpdatedAt);

        return Results.Created($"/api/profiles/{profileId}/rules/{result.Id}", result);
    }

    [WolverineGet("/api/profiles/{profileId}/rules")]
    public static async Task<IResult> GetRules(
        Guid profileId,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var profileExists = await db.Profiles
            .AnyAsync(p => p.Id == profileId && p.User!.ExternalId == currentUser.Id, ct);

        if (!profileExists)
            return Results.NotFound();

        var rules = await db.CategorizationRules
            .Where(r => r.ProfileId == profileId)
            .OrderBy(r => r.Priority)
            .Select(r => new RuleDto(
                r.Id,
                r.Name,
                r.Priority,
                r.RuleToml,
                r.IsActive,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(ct);

        return Results.Ok(rules);
    }

    [WolverineGet("/api/rules/{id}")]
    public static async Task<IResult> GetRule(
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var rule = await db.CategorizationRules
            .Where(r => r.Id == id && r.Profile!.User!.ExternalId == currentUser.Id)
            .Select(r => new RuleDto(
                r.Id,
                r.Name,
                r.Priority,
                r.RuleToml,
                r.IsActive,
                r.CreatedAt,
                r.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return rule is null ? Results.NotFound() : Results.Ok(rule);
    }

    [WolverinePut("/api/rules/{id}")]
    public static async Task<IResult> UpdateRule(
        Guid id,
        [FromBody] UpdateRuleRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        IRulesEngine rulesEngine,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var rule = await db.CategorizationRules
            .Where(r => r.Id == id && r.Profile!.User!.ExternalId == currentUser.Id)
            .FirstOrDefaultAsync(ct);

        if (rule is null)
            return Results.NotFound();

        var validation = rulesEngine.ValidateRule(request.RuleToml);
        if (!validation.IsValid)
            return Results.BadRequest(new { error = validation.ErrorMessage });

        rule.Name = request.Name;
        rule.Priority = request.Priority;
        rule.RuleToml = request.RuleToml;
        rule.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);

        var result = new RuleDto(
            rule.Id,
            rule.Name,
            rule.Priority,
            rule.RuleToml,
            rule.IsActive,
            rule.CreatedAt,
            rule.UpdatedAt);

        return Results.Ok(result);
    }

    [WolverineDelete("/api/rules/{id}")]
    public static async Task<IResult> DeleteRule(
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var rule = await db.CategorizationRules
            .Where(r => r.Id == id && r.Profile!.User!.ExternalId == currentUser.Id)
            .FirstOrDefaultAsync(ct);

        if (rule is null)
            return Results.NotFound();

        db.CategorizationRules.Remove(rule);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    [WolverinePost("/api/profiles/{profileId}/rules/test")]
    public static async Task<IResult> TestRules(
        Guid profileId,
        [FromBody] TestRulesRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        IRulesEngine rulesEngine,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var profileExists = await db.Profiles
            .AnyAsync(p => p.Id == profileId && p.User!.ExternalId == currentUser.Id, ct);

        if (!profileExists)
            return Results.NotFound();

        var rules = await db.CategorizationRules
            .Where(r => r.ProfileId == profileId && r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        // Create a dummy transaction for testing
        var testTransaction = new Transaction
        {
            AccountId = Guid.Empty,
            Date = request.Date,
            Amount = request.Amount,
            Description = request.Description
        };

        var match = await rulesEngine.MatchAsync(testTransaction, rules, ct);

        return Results.Ok(new TestRulesResponse(
            match?.MatchedRuleName,
            match?.CategoryName,
            match?.Tags ?? []));
    }

    [WolverinePost("/api/profiles/{profileId}/rules/apply")]
    public static async Task<IResult> ApplyRules(
        Guid profileId,
        [FromBody] ApplyRulesRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        IRulesEngine rulesEngine,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var profileExists = await db.Profiles
            .AnyAsync(p => p.Id == profileId && p.User!.ExternalId == currentUser.Id, ct);

        if (!profileExists)
            return Results.NotFound();

        var rules = await db.CategorizationRules
            .Where(r => r.ProfileId == profileId && r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        if (rules.Count == 0)
            return Results.Ok(new ApplyRulesResponse(0));

        var transactionsQuery = db.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account!.ProfileId == profileId);

        if (request.OnlyUncategorized)
            transactionsQuery = transactionsQuery.Where(t => t.CategoryId == null);

        var transactions = await transactionsQuery.ToListAsync(ct);

        // Build category lookup
        var categories = await db.Categories
            .Where(c => c.ProfileId == profileId)
            .ToDictionaryAsync(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase, ct);

        Guid? CategoryLookup(string name) =>
            categories.TryGetValue(name, out var id) ? id : null;

        var updated = await rulesEngine.ApplyRulesAsync(transactions, rules, CategoryLookup, ct);

        await db.SaveChangesAsync(ct);

        return Results.Ok(new ApplyRulesResponse(updated));
    }
}
