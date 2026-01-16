using FinTrack.Core.Features.Nlq;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

public static class NlqEndpoints
{
    [WolverinePost("/api/profiles/{profileId}/nlq/query")]
    [Tags("NLQ")]
    [EndpointSummary("Execute natural language query")]
    [EndpointDescription("Translates a natural language question to SQL and executes it against the user's financial data.")]
    [ProducesResponseType<NlqResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> ExecuteQuery(
        Guid profileId,
        [FromBody] NlqRequest request,
        FinTrackDbContext db,
        INlqService nlqService,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profileExists = await db.Profiles
            .AnyAsync(p => p.Id == profileId && p.UserId == userId, ct);

        if (!profileExists)
            return Results.NotFound();

        if (string.IsNullOrWhiteSpace(request.Question))
            return Results.BadRequest("Question is required");

        var result = await nlqService.ExecuteQueryAsync(profileId, request.Question, ct);

        return Results.Ok(result);
    }

    [WolverineGet("/api/profiles/{profileId}/nlq/suggestions")]
    [Tags("NLQ")]
    [EndpointSummary("Get query suggestions")]
    [EndpointDescription("Returns suggested questions the user can ask based on their data.")]
    [ProducesResponseType<List<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetSuggestions(
        Guid profileId,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profileExists = await db.Profiles
            .AnyAsync(p => p.Id == profileId && p.UserId == userId, ct);

        if (!profileExists)
            return Results.NotFound();

        // Get some context to personalize suggestions
        var hasTransactions = await db.Transactions
            .AnyAsync(t => t.Account!.ProfileId == profileId, ct);

        var categories = await db.Categories
            .Where(c => c.ProfileId == profileId)
            .Select(c => c.Name)
            .Take(3)
            .ToListAsync(ct);

        var suggestions = new List<string>
        {
            "How much did I spend last month?",
            "What are my top 5 expenses this year?",
            "Show my income vs expenses for the last 6 months",
            "What is my average monthly spending?"
        };

        if (categories.Count > 0)
        {
            suggestions.Add($"How much did I spend on {categories[0]} last month?");
        }

        if (!hasTransactions)
        {
            suggestions.Insert(0, "How many transactions do I have?");
        }

        return Results.Ok(suggestions);
    }
}
