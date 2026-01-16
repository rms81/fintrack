using System.Text.Json;
using FinTrack.Core.Features.Export;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

/// <summary>
/// Endpoints for exporting and importing profile data.
/// </summary>
public static class ExportEndpoints
{
    [WolverineGet("/api/profiles/{profileId}/export/json")]
    [Tags("Export")]
    [EndpointSummary("Export profile as JSON")]
    [EndpointDescription("Exports the entire profile including accounts, categories, rules, import formats, and transactions as a JSON file for backup or migration.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> ExportJson(
        Guid profileId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] bool includeRules = true,
        [FromQuery] bool includeFormats = true,
        FinTrackDbContext db = null!,
        ICurrentUser currentUser = null!,
        IExportService exportService = null!,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profile = await db.Profiles
            .AsNoTracking()
            .Where(p => p.Id == profileId && p.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return Results.NotFound();

        var options = new JsonExportOptions(fromDate, toDate, includeRules, includeFormats);
        var exportData = await exportService.ExportProfileJsonAsync(profileId, options, ct);

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var fileName = $"fintrack-{SanitizeFileName(profile.Name)}-{DateTime.UtcNow:yyyy-MM-dd}.json";

        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(json),
            "application/json",
            fileName);
    }

    [WolverineGet("/api/profiles/{profileId}/export/csv")]
    [Tags("Export")]
    [EndpointSummary("Export transactions as CSV")]
    [EndpointDescription("Exports transactions as a CSV file compatible with Excel and Google Sheets. Supports filtering by date range, account, and category.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> ExportCsv(
        Guid profileId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? categoryId,
        FinTrackDbContext db = null!,
        ICurrentUser currentUser = null!,
        IExportService exportService = null!,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var profile = await db.Profiles
            .AsNoTracking()
            .Where(p => p.Id == profileId && p.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return Results.NotFound();

        var options = new CsvExportOptions(fromDate, toDate, accountId, categoryId);
        var csvBytes = await exportService.ExportTransactionsCsvAsync(profileId, options, ct);

        var fileName = $"fintrack-transactions-{DateTime.UtcNow:yyyy-MM-dd}.csv";

        return Results.File(csvBytes, "text/csv", fileName);
    }

    [WolverinePost("/api/import/json/preview")]
    [Tags("Export")]
    [EndpointSummary("Preview JSON import")]
    [EndpointDescription("Uploads a FinTrack JSON export file and returns a preview of what will be imported, including counts and any warnings.")]
    [ProducesResponseType<JsonImportPreviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> PreviewJsonImport(
        IFormFile file,
        ICurrentUser currentUser = null!,
        IExportService exportService = null!,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated)
            return Results.Unauthorized();

        if (file.Length == 0)
            return Results.BadRequest(new { error = "Empty file" });

        if (file.Length > 100 * 1024 * 1024) // 100MB limit for imports
            return Results.BadRequest(new { error = "File too large (max 100MB)" });

        try
        {
            await using var stream = file.OpenReadStream();
            var (preview, sessionId) = await exportService.PreviewJsonImportAsync(stream, ct);

            return Results.Ok(new
            {
                sessionId,
                preview.ProfileName,
                preview.ProfileType,
                preview.AccountCount,
                preview.CategoryCount,
                preview.RuleCount,
                preview.ImportFormatCount,
                preview.TransactionCount,
                preview.Warnings
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    [WolverinePost("/api/import/json/confirm")]
    [Tags("Export")]
    [EndpointSummary("Confirm JSON import")]
    [EndpointDescription("Confirms and executes the JSON import, creating a new profile with all imported data. IDs are regenerated to avoid conflicts.")]
    [ProducesResponseType<JsonImportResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> ConfirmJsonImport(
        [FromBody] JsonImportConfirmRequest request,
        ICurrentUser currentUser = null!,
        IExportService exportService = null!,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(request.ProfileName))
            return Results.BadRequest(new { error = "Profile name is required" });

        try
        {
            var result = await exportService.ConfirmJsonImportAsync(
                request.SessionId,
                userId,
                request,
                ct);

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("", name.Select(c => invalid.Contains(c) ? '_' : c)).ToLowerInvariant();
    }
}
