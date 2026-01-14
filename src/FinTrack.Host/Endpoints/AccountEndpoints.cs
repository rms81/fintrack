using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Features.Accounts;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

/// <summary>
/// Endpoints for managing bank accounts within profiles.
/// </summary>
public static class AccountEndpoints
{
    [WolverinePost("/api/profiles/{profileId}/accounts")]
    [Tags("Accounts")]
    [EndpointSummary("Create a new account")]
    [EndpointDescription("Creates a new bank account within the specified profile. Accounts are used to organize transactions by source (e.g., checking, savings, credit card).")]
    [ProducesResponseType<AccountDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> CreateAccount(
        Guid profileId,
        [FromBody] CreateAccountRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        // Verify profile belongs to user
        var profile = await db.Profiles
            .Where(p => p.Id == profileId && p.User!.ExternalId == currentUser.Id)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return Results.NotFound();

        var account = new Account
        {
            ProfileId = profileId,
            Name = request.Name,
            BankName = request.BankName,
            Currency = request.Currency
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);

        var result = new AccountDto(
            account.Id,
            account.ProfileId,
            account.Name,
            account.BankName,
            account.Currency,
            account.CreatedAt,
            account.UpdatedAt);

        return Results.Created($"/api/profiles/{profileId}/accounts/{result.Id}", result);
    }

    [WolverineGet("/api/profiles/{profileId}/accounts")]
    [Tags("Accounts")]
    [EndpointSummary("List accounts in a profile")]
    [EndpointDescription("Returns all bank accounts within the specified profile, ordered by name.")]
    [ProducesResponseType<List<AccountDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetAccounts(
        Guid profileId,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        // Verify profile belongs to user
        var profileExists = await db.Profiles
            .AnyAsync(p => p.Id == profileId && p.User!.ExternalId == currentUser.Id, ct);

        if (!profileExists)
            return Results.NotFound();

        var accounts = await db.Accounts
            .Where(a => a.ProfileId == profileId)
            .OrderBy(a => a.Name)
            .Select(a => new AccountDto(
                a.Id,
                a.ProfileId,
                a.Name,
                a.BankName,
                a.Currency,
                a.CreatedAt,
                a.UpdatedAt))
            .ToListAsync(ct);

        return Results.Ok(accounts);
    }

    [WolverineGet("/api/profiles/{profileId}/accounts/{id}")]
    [Tags("Accounts")]
    [EndpointSummary("Get an account by ID")]
    [EndpointDescription("Returns a single account by its unique identifier.")]
    [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetAccount(
        Guid profileId,
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var account = await db.Accounts
            .Where(a => a.Id == id && a.ProfileId == profileId && a.Profile!.User!.ExternalId == currentUser.Id)
            .Select(a => new AccountDto(
                a.Id,
                a.ProfileId,
                a.Name,
                a.BankName,
                a.Currency,
                a.CreatedAt,
                a.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return account is null ? Results.NotFound() : Results.Ok(account);
    }

    [WolverinePut("/api/profiles/{profileId}/accounts/{id}")]
    [Tags("Accounts")]
    [EndpointSummary("Update an account")]
    [EndpointDescription("Updates an existing account's name, bank name, and currency.")]
    [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> UpdateAccount(
        Guid profileId,
        Guid id,
        [FromBody] UpdateAccountRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var account = await db.Accounts
            .Where(a => a.Id == id && a.ProfileId == profileId && a.Profile!.User!.ExternalId == currentUser.Id)
            .FirstOrDefaultAsync(ct);

        if (account is null)
            return Results.NotFound();

        account.Name = request.Name;
        account.BankName = request.BankName;
        account.Currency = request.Currency;

        await db.SaveChangesAsync(ct);

        var result = new AccountDto(
            account.Id,
            account.ProfileId,
            account.Name,
            account.BankName,
            account.Currency,
            account.CreatedAt,
            account.UpdatedAt);

        return Results.Ok(result);
    }

    [WolverineDelete("/api/profiles/{profileId}/accounts/{id}")]
    [Tags("Accounts")]
    [EndpointSummary("Delete an account")]
    [EndpointDescription("Permanently deletes an account and all its transactions.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> DeleteAccount(
        Guid profileId,
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var account = await db.Accounts
            .Where(a => a.Id == id && a.ProfileId == profileId && a.Profile!.User!.ExternalId == currentUser.Id)
            .FirstOrDefaultAsync(ct);

        if (account is null)
            return Results.NotFound();

        db.Accounts.Remove(account);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
