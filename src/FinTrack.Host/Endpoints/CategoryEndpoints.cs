using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Features.Categories;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

/// <summary>
/// Endpoints for managing transaction categories.
/// </summary>
public static class CategoryEndpoints
{
    [WolverinePost("/api/profiles/{profileId}/categories")]
    [Tags("Categories")]
    [EndpointSummary("Create a new category")]
    [EndpointDescription("Creates a new category for organizing transactions. Categories can be nested using the parentId field.")]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> CreateCategory(
        Guid profileId,
        [FromBody] CreateCategoryRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profile = await db.Profiles
            .Where(p => p.Id == profileId && p.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return Results.NotFound();

        // Verify parent category belongs to same profile if specified
        if (request.ParentId.HasValue)
        {
            var parentExists = await db.Categories
                .AnyAsync(c => c.Id == request.ParentId.Value && c.ProfileId == profileId, ct);
            if (!parentExists)
                return Results.BadRequest(new { error = "Parent category not found" });
        }

        var category = new Category
        {
            ProfileId = profileId,
            Name = request.Name,
            Icon = request.Icon,
            Color = request.Color,
            ParentId = request.ParentId
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        var result = new CategoryDto(
            category.Id,
            category.Name,
            category.Icon,
            category.Color,
            category.ParentId,
            0,
            category.CreatedAt,
            category.UpdatedAt);

        return Results.Created($"/api/profiles/{profileId}/categories/{result.Id}", result);
    }

    [WolverineGet("/api/profiles/{profileId}/categories")]
    [Tags("Categories")]
    [EndpointSummary("List categories in a profile")]
    [EndpointDescription("Returns all categories within the specified profile, including transaction counts.")]
    [ProducesResponseType<List<CategoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetCategories(
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

        var categories = await db.Categories
            .Where(c => c.ProfileId == profileId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Icon,
                c.Color,
                c.ParentId,
                c.Transactions.Count,
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(ct);

        return Results.Ok(categories);
    }

    [WolverineGet("/api/profiles/{profileId}/categories/{id}")]
    [Tags("Categories")]
    [EndpointSummary("Get a category by ID")]
    [EndpointDescription("Returns a single category by its unique identifier.")]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetCategory(
        Guid profileId,
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var category = await db.Categories
            .Where(c => c.Id == id && c.ProfileId == profileId && c.Profile!.UserId == userId)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Icon,
                c.Color,
                c.ParentId,
                c.Transactions.Count,
                c.CreatedAt,
                c.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return category is null ? Results.NotFound() : Results.Ok(category);
    }

    [WolverinePut("/api/profiles/{profileId}/categories/{id}")]
    [Tags("Categories")]
    [EndpointSummary("Update a category")]
    [EndpointDescription("Updates an existing category's name, icon, color, and parent.")]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> UpdateCategory(
        Guid profileId,
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var category = await db.Categories
            .Where(c => c.Id == id && c.ProfileId == profileId && c.Profile!.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (category is null)
            return Results.NotFound();

        // Verify parent category if specified
        if (request.ParentId.HasValue && request.ParentId.Value != category.Id)
        {
            var parentExists = await db.Categories
                .AnyAsync(c => c.Id == request.ParentId.Value && c.ProfileId == profileId, ct);
            if (!parentExists)
                return Results.BadRequest(new { error = "Parent category not found" });
        }

        category.Name = request.Name;
        category.Icon = request.Icon;
        category.Color = request.Color;
        category.ParentId = request.ParentId;

        await db.SaveChangesAsync(ct);

        var transactionCount = await db.Transactions.CountAsync(t => t.CategoryId == category.Id, ct);

        var result = new CategoryDto(
            category.Id,
            category.Name,
            category.Icon,
            category.Color,
            category.ParentId,
            transactionCount,
            category.CreatedAt,
            category.UpdatedAt);

        return Results.Ok(result);
    }

    [WolverineDelete("/api/profiles/{profileId}/categories/{id}")]
    [Tags("Categories")]
    [EndpointSummary("Delete a category")]
    [EndpointDescription("Permanently deletes a category. Transactions in this category will become uncategorized.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> DeleteCategory(
        Guid profileId,
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var category = await db.Categories
            .Where(c => c.Id == id && c.ProfileId == profileId && c.Profile!.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (category is null)
            return Results.NotFound();

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
