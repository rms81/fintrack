using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Domain.Enums;
using FinTrack.Core.Domain.ValueObjects;
using FinTrack.Core.Features.Import;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Caching;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

/// <summary>
/// Endpoints for importing bank statement CSV files.
/// </summary>
public static class ImportEndpoints
{
    [WolverinePost("/api/accounts/{accountId}/import/upload")]
    [Tags("Import")]
    [EndpointSummary("Upload a CSV file for import")]
    [EndpointDescription("Uploads a bank statement CSV file and analyzes its format using LLM-assisted detection. Returns the detected format configuration and sample rows for preview.")]
    [ProducesResponseType<UploadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> UploadCsv(
        Guid accountId,
        IFormFile file,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        IImportService importService,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var account = await db.Accounts
            .Include(a => a.Profile)
            .Where(a => a.Id == accountId && a.Profile!.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (account is null)
            return Results.NotFound();

        if (file.Length == 0)
            return Results.BadRequest(new { error = "Empty file" });

        if (file.Length > 10 * 1024 * 1024) // 10MB limit
            return Results.BadRequest(new { error = "File too large (max 10MB)" });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var csvData = ms.ToArray();

        var (format, sampleRows, rowCount) = await importService.AnalyzeCsvAsync(csvData, ct);

        var session = new ImportSession
        {
            AccountId = accountId,
            Filename = file.FileName,
            RowCount = rowCount,
            Status = ImportStatus.Pending,
            FormatConfig = format,
            CsvData = csvData
        };

        db.ImportSessions.Add(session);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new UploadResponse(
            session.Id,
            session.Filename,
            rowCount,
            format,
            sampleRows));
    }

    [WolverinePost("/api/import/{sessionId}/preview")]
    [Tags("Import")]
    [EndpointSummary("Preview import results")]
    [EndpointDescription("Parses the uploaded CSV and returns a preview of transactions that will be imported, including duplicate detection.")]
    [ProducesResponseType<PreviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> PreviewImport(
        Guid sessionId,
        [FromBody] PreviewRequest? request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        IImportService importService,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var session = await db.ImportSessions
            .Include(s => s.Account)
                .ThenInclude(a => a!.Profile)
            .Where(s => s.Id == sessionId && s.Account!.Profile!.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (session?.CsvData is null)
            return Results.NotFound();

        var format = request?.FormatOverride ?? session.FormatConfig!;

        // Get existing transaction hashes for duplicate detection
        var existingHashes = await db.Transactions
            .Where(t => t.AccountId == session.AccountId && t.DuplicateHash != null)
            .Select(t => t.DuplicateHash!)
            .ToListAsync(ct);

        var hashSet = existingHashes.ToHashSet();

        var transactions = importService.ParseTransactions(session.CsvData, format, hashSet);
        var duplicateCount = transactions.Count(t => t.IsDuplicate);

        return Results.Ok(new PreviewResponse(sessionId, transactions, duplicateCount));
    }

    [WolverinePost("/api/import/{sessionId}/confirm")]
    [Tags("Import")]
    [EndpointSummary("Confirm and execute import")]
    [EndpointDescription("Confirms the import session and creates transactions in the database. Automatically applies categorization rules to new transactions.")]
    [ProducesResponseType<ConfirmImportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public static async Task<IResult> ConfirmImport(
        Guid sessionId,
        [FromBody] ConfirmImportRequest? request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        IImportService importService,
        IRulesEngine rulesEngine,
        ICacheService cache,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var session = await db.ImportSessions
            .Include(s => s.Account)
                .ThenInclude(a => a!.Profile)
            .Where(s => s.Id == sessionId && s.Account!.Profile!.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (session?.CsvData is null)
            return Results.NotFound();

        if (session.Status == ImportStatus.Completed)
            return Results.BadRequest(new { error = "Import already completed" });

        var format = request?.FormatOverride ?? session.FormatConfig!;
        var skipDuplicates = request?.SkipDuplicates ?? true;

        try
        {
            session.Status = ImportStatus.Processing;
            await db.SaveChangesAsync(ct);

            // Get existing transaction hashes
            var existingHashes = await db.Transactions
                .Where(t => t.AccountId == session.AccountId && t.DuplicateHash != null)
                .Select(t => t.DuplicateHash!)
                .ToListAsync(ct);

            var hashSet = existingHashes.ToHashSet();

            var transactions = importService.CreateTransactions(
                session.CsvData, format, session.AccountId, hashSet, skipDuplicates);

            var skippedCount = session.RowCount - (format.HasHeader ? 1 : 0) - transactions.Count;

            db.Transactions.AddRange(transactions);

            // Apply categorization rules to new transactions
            var profileId = session.Account!.ProfileId;
            var rules = await db.CategorizationRules
                .Where(r => r.ProfileId == profileId && r.IsActive)
                .OrderBy(r => r.Priority)
                .ToListAsync(ct);

            if (rules.Count > 0)
            {
                var categories = await db.Categories
                    .Where(c => c.ProfileId == profileId)
                    .ToDictionaryAsync(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase, ct);

                Guid? CategoryLookup(string name) =>
                    categories.TryGetValue(name, out var id) ? id : null;

                await rulesEngine.ApplyRulesAsync(transactions, rules, CategoryLookup, ct);
            }

            session.Status = ImportStatus.Completed;
            session.CsvData = null; // Clear stored data after import

            await db.SaveChangesAsync(ct);

            // Invalidate dashboard and categories caches after import
            cache.RemoveByPrefix($"fintrack:dashboard:summary:{profileId}:");
            cache.RemoveByPrefix($"fintrack:dashboard:category-spending:{profileId}:");
            cache.RemoveByPrefix($"fintrack:dashboard:top-merchants:{profileId}:");
            cache.Remove(CacheKeys.Categories(profileId));

            return Results.Ok(new ConfirmImportResponse(transactions.Count, skippedCount));
        }
        catch (Exception ex)
        {
            session.Status = ImportStatus.Failed;
            session.ErrorMessage = ex.Message;
            await db.SaveChangesAsync(ct);

            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Import failed");
        }
    }

    [WolverineGet("/api/accounts/{accountId}/import/sessions")]
    [Tags("Import")]
    [EndpointSummary("List import sessions")]
    [EndpointDescription("Returns the import history for an account, including status and any error messages.")]
    [ProducesResponseType<List<ImportSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetImportSessions(
        Guid accountId,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var accountExists = await db.Accounts
            .AnyAsync(a => a.Id == accountId && a.Profile!.UserId == userId, ct);

        if (!accountExists)
            return Results.NotFound();

        var sessions = await db.ImportSessions
            .Where(s => s.AccountId == accountId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new ImportSessionDto(
                s.Id,
                s.AccountId,
                s.Filename,
                s.RowCount,
                s.Status,
                s.ErrorMessage,
                s.FormatConfig,
                s.CreatedAt,
                s.UpdatedAt))
            .ToListAsync(ct);

        return Results.Ok(sessions);
    }

    // Import Format CRUD endpoints

    [WolverineGet("/api/profiles/{profileId}/import-formats")]
    [Tags("Import Formats")]
    [EndpointSummary("List saved import formats")]
    [EndpointDescription("Returns all saved CSV import formats for a profile, which can be reused for future imports.")]
    [ProducesResponseType<List<ImportFormatDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetImportFormats(
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

        var formats = await db.ImportFormats
            .Where(f => f.ProfileId == profileId)
            .OrderBy(f => f.Name)
            .Select(f => new ImportFormatDto(
                f.Id,
                f.Name,
                f.BankName,
                f.Mapping,
                f.CreatedAt,
                f.UpdatedAt))
            .ToListAsync(ct);

        return Results.Ok(formats);
    }

    [WolverinePost("/api/profiles/{profileId}/import-formats")]
    [Tags("Import Formats")]
    [EndpointSummary("Save an import format")]
    [EndpointDescription("Saves a CSV import format configuration for reuse with future imports from the same bank.")]
    [ProducesResponseType<ImportFormatDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> CreateImportFormat(
        Guid profileId,
        [FromBody] CreateImportFormatRequest request,
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

        // Check for duplicate name
        var nameExists = await db.ImportFormats
            .AnyAsync(f => f.ProfileId == profileId && f.Name == request.Name, ct);

        if (nameExists)
            return Results.BadRequest(new { error = "A format with this name already exists" });

        var format = new ImportFormat
        {
            ProfileId = profileId,
            Name = request.Name,
            BankName = request.BankName,
            Mapping = request.Mapping
        };

        db.ImportFormats.Add(format);
        await db.SaveChangesAsync(ct);

        var result = new ImportFormatDto(
            format.Id,
            format.Name,
            format.BankName,
            format.Mapping,
            format.CreatedAt,
            format.UpdatedAt);

        return Results.Created($"/api/profiles/{profileId}/import-formats/{result.Id}", result);
    }

    [WolverineGet("/api/import-formats/{id}")]
    [Tags("Import Formats")]
    [EndpointSummary("Get an import format by ID")]
    [EndpointDescription("Returns a single import format by its unique identifier.")]
    [ProducesResponseType<ImportFormatDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetImportFormat(
        Guid id,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var format = await db.ImportFormats
            .Where(f => f.Id == id && f.Profile!.UserId == userId)
            .Select(f => new ImportFormatDto(
                f.Id,
                f.Name,
                f.BankName,
                f.Mapping,
                f.CreatedAt,
                f.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return format is null ? Results.NotFound() : Results.Ok(format);
    }

    [WolverinePut("/api/import-formats/{id}")]
    [Tags("Import Formats")]
    [EndpointSummary("Update an import format")]
    [EndpointDescription("Updates an existing import format's name, bank name, and column mappings.")]
    [ProducesResponseType<ImportFormatDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> UpdateImportFormat(
        Guid id,
        [FromBody] UpdateImportFormatRequest request,
        FinTrackDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var format = await db.ImportFormats
            .Where(f => f.Id == id && f.Profile!.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (format is null)
            return Results.NotFound();

        // Check for duplicate name (excluding current format)
        var nameExists = await db.ImportFormats
            .AnyAsync(f => f.ProfileId == format.ProfileId && f.Name == request.Name && f.Id != id, ct);

        if (nameExists)
            return Results.BadRequest(new { error = "A format with this name already exists" });

        format.Name = request.Name;
        format.BankName = request.BankName;
        format.Mapping = request.Mapping;

        await db.SaveChangesAsync(ct);

        var result = new ImportFormatDto(
            format.Id,
            format.Name,
            format.BankName,
            format.Mapping,
            format.CreatedAt,
            format.UpdatedAt);

        return Results.Ok(result);
    }

    [WolverineDelete("/api/import-formats/{id}")]
    [Tags("Import Formats")]
    [EndpointSummary("Delete an import format")]
    [EndpointDescription("Permanently deletes a saved import format.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> DeleteImportFormat(
        Guid id,
        [FromServices] FinTrackDbContext db,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.Id, out var userId))
            return Results.Unauthorized();

        var format = await db.ImportFormats
            .Where(f => f.Id == id && f.Profile!.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (format is null)
            return Results.NotFound();

        db.ImportFormats.Remove(format);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
