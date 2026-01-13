using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Features.Profiles;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

public static class ProfileEndpoints
{
    [WolverinePost("/api/profiles")]
    public static async Task<IResult> CreateProfile(
        [FromBody] CreateProfileRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        // Get or create user
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.ExternalId == currentUser.Id, ct);

        if (user is null)
        {
            user = new User
            {
                ExternalId = currentUser.Id,
                Email = currentUser.Email ?? "unknown@example.com",
                DisplayName = currentUser.DisplayName
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
        }

        var profile = new Profile
        {
            UserId = user.Id,
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
    public static async Task<IResult> GetProfiles(
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var profiles = await db.Profiles
            .Where(p => p.User!.ExternalId == currentUser.Id)
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
    public static async Task<IResult> GetProfile(
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var profile = await db.Profiles
            .Where(p => p.Id == id && p.User!.ExternalId == currentUser.Id)
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
    public static async Task<IResult> UpdateProfile(
        Guid id,
        [FromBody] UpdateProfileRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var profile = await db.Profiles
            .Where(p => p.Id == id && p.User!.ExternalId == currentUser.Id)
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
    public static async Task<IResult> DeleteProfile(
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Id is null)
            return Results.Unauthorized();

        var profile = await db.Profiles
            .Where(p => p.Id == id && p.User!.ExternalId == currentUser.Id)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return Results.NotFound();

        db.Profiles.Remove(profile);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
