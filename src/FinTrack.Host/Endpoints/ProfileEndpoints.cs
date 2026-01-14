using System.ComponentModel;
using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Features.Profiles;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

/// <summary>
/// Endpoints for managing user profiles (Personal/Business).
/// </summary>
public static class ProfileEndpoints
{
    [WolverinePost("/api/profiles")]
    [Tags("Profiles")]
    [EndpointSummary("Create a new profile")]
    [EndpointDescription("Creates a new profile for the authenticated user. Each user can have multiple profiles (e.g., Personal, Business).")]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> CreateProfile(
        [FromBody] CreateProfileRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profile = new Profile
        {
            UserId = userId,
            Name = request.Name,
            Type = request.Type
        };

        db.Profiles.Add(profile);
        await db.SaveChangesAsync(ct);

        var result = new ProfileDto(
            profile.Id,
            profile.Name,
            profile.Type,
            profile.CreatedAt,
            profile.UpdatedAt);

        return Results.Created($"/api/profiles/{result.Id}", result);
    }

    [WolverineGet("/api/profiles")]
    [Tags("Profiles")]
    [EndpointSummary("List all profiles")]
    [EndpointDescription("Returns all profiles owned by the authenticated user, ordered by name.")]
    [ProducesResponseType<List<ProfileDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> GetProfiles(
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profiles = await db.Profiles
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .Select(p => new ProfileDto(
                p.Id,
                p.Name,
                p.Type,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(ct);

        return Results.Ok(profiles);
    }

    [WolverineGet("/api/profiles/{id}")]
    [Tags("Profiles")]
    [EndpointSummary("Get a profile by ID")]
    [EndpointDescription("Returns a single profile by its unique identifier.")]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetProfile(
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profile = await db.Profiles
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(p => new ProfileDto(
                p.Id,
                p.Name,
                p.Type,
                p.CreatedAt,
                p.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return profile is null ? Results.NotFound() : Results.Ok(profile);
    }

    [WolverinePut("/api/profiles/{id}")]
    [Tags("Profiles")]
    [EndpointSummary("Update a profile")]
    [EndpointDescription("Updates an existing profile's name and type.")]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> UpdateProfile(
        Guid id,
        [FromBody] UpdateProfileRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profile = await db.Profiles
            .Where(p => p.Id == id && p.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return Results.NotFound();

        profile.Name = request.Name;
        profile.Type = request.Type;

        await db.SaveChangesAsync(ct);

        var result = new ProfileDto(
            profile.Id,
            profile.Name,
            profile.Type,
            profile.CreatedAt,
            profile.UpdatedAt);

        return Results.Ok(result);
    }

    [WolverineDelete("/api/profiles/{id}")]
    [Tags("Profiles")]
    [EndpointSummary("Delete a profile")]
    [EndpointDescription("Permanently deletes a profile and all associated data (accounts, transactions, categories, rules).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> DeleteProfile(
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profile = await db.Profiles
            .Where(p => p.Id == id && p.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return Results.NotFound();

        db.Profiles.Remove(profile);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
