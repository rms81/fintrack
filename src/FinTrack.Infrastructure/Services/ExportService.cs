using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Features.Export;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Services;

public class ExportService(
    FinTrackDbContext db,
    ILogger<ExportService> logger) : IExportService
{
    // In-memory session storage (in production, consider distributed cache)
    private static readonly ConcurrentDictionary<Guid, JsonImportSession> ImportSessions = new();

    public async Task<ProfileExportData> ExportProfileJsonAsync(
        Guid profileId,
        JsonExportOptions options,
        CancellationToken ct = default)
    {
        var profile = await db.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == profileId, ct)
            ?? throw new InvalidOperationException($"Profile {profileId} not found");

        var accounts = await db.Accounts
            .AsNoTracking()
            .Where(a => a.ProfileId == profileId)
            .ToListAsync(ct);

        var categories = await db.Categories
            .AsNoTracking()
            .Where(c => c.ProfileId == profileId)
            .OrderBy(c => c.ParentId == null ? 0 : 1)
            .ThenBy(c => c.SortOrder)
            .ToListAsync(ct);

        var rules = options.IncludeRules
            ? await db.CategorizationRules
                .AsNoTracking()
                .Where(r => r.ProfileId == profileId)
                .OrderBy(r => r.Priority)
                .ToListAsync(ct)
            : [];

        var importFormats = options.IncludeFormats
            ? await db.ImportFormats
                .AsNoTracking()
                .Where(f => f.ProfileId == profileId)
                .ToListAsync(ct)
            : [];

        var accountIds = accounts.Select(a => a.Id).ToList();
        var transactionsQuery = db.Transactions
            .AsNoTracking()
            .Where(t => accountIds.Contains(t.AccountId));

        if (options.FromDate.HasValue)
            transactionsQuery = transactionsQuery.Where(t => t.Date >= options.FromDate.Value);
        if (options.ToDate.HasValue)
            transactionsQuery = transactionsQuery.Where(t => t.Date <= options.ToDate.Value);

        var transactions = await transactionsQuery
            .OrderBy(t => t.Date)
            .ToListAsync(ct);

        logger.LogInformation(
            "Exporting profile {ProfileId}: {AccountCount} accounts, {CategoryCount} categories, {TransactionCount} transactions",
            profileId, accounts.Count, categories.Count, transactions.Count);

        return new ProfileExportData
        {
            ExportedAt = DateTime.UtcNow,
            Profile = new ProfileExportInfo(profile.Name, profile.Type),
            Accounts = accounts.Select(a => new AccountExportInfo(
                a.Id, a.Name, a.BankName, a.Currency)).ToList(),
            Categories = categories.Select(c => new CategoryExportInfo(
                c.Id, c.Name, c.Icon, c.Color, c.SortOrder, c.ParentId)).ToList(),
            Rules = rules.Select(r => new RuleExportInfo(
                r.Name, r.Priority, r.RuleToml, r.IsActive)).ToList(),
            ImportFormats = importFormats.Select(f => new ImportFormatExportInfo(
                f.Name, f.BankName, f.Mapping)).ToList(),
            Transactions = transactions.Select(t => new TransactionExportInfo(
                t.AccountId, t.Date, t.Amount, t.Description, t.Notes, t.Tags, t.CategoryId)).ToList()
        };
    }

    public async Task<byte[]> ExportTransactionsCsvAsync(
        Guid profileId,
        CsvExportOptions options,
        CancellationToken ct = default)
    {
        var accountIds = await db.Accounts
            .AsNoTracking()
            .Where(a => a.ProfileId == profileId)
            .Select(a => a.Id)
            .ToListAsync(ct);

        var query = db.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.Category)
            .ThenInclude(c => c!.Parent)
            .Where(t => accountIds.Contains(t.AccountId));

        if (options.FromDate.HasValue)
            query = query.Where(t => t.Date >= options.FromDate.Value);
        if (options.ToDate.HasValue)
            query = query.Where(t => t.Date <= options.ToDate.Value);
        if (options.AccountId.HasValue)
            query = query.Where(t => t.AccountId == options.AccountId.Value);
        if (options.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == options.CategoryId.Value);

        var transactions = await query
            .OrderByDescending(t => t.Date)
            .ToListAsync(ct);

        var sb = new StringBuilder();

        // UTF-8 BOM for Excel compatibility
        sb.Append('\uFEFF');

        // Header
        sb.AppendLine("Date,Description,Amount,Category,Subcategory,Tags,Account,Notes");

        foreach (var t in transactions)
        {
            var category = t.Category?.Name;
            var subcategory = t.Category?.Parent?.Name;
            var tags = string.Join(";", t.Tags);
            var account = t.Account?.Name;

            sb.AppendLine(string.Join(",",
                t.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                CsvEscape(t.Description),
                t.Amount.ToString("F2", CultureInfo.InvariantCulture),
                CsvEscape(category),
                CsvEscape(subcategory),
                CsvEscape(tags),
                CsvEscape(account),
                CsvEscape(t.Notes)));
        }

        logger.LogInformation("Exported {Count} transactions to CSV", transactions.Count);

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<(JsonImportPreviewResponse Preview, Guid SessionId)> PreviewJsonImportAsync(
        Stream jsonStream,
        CancellationToken ct = default)
    {
        ProfileExportData data;
        try
        {
            data = await JsonSerializer.DeserializeAsync<ProfileExportData>(
                jsonStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                ct) ?? throw new InvalidOperationException("Invalid JSON format");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON: {ex.Message}", ex);
        }

        var warnings = new List<string>();

        // Validate version
        if (data.Version != "1.0")
        {
            warnings.Add($"Export version {data.Version} may not be fully compatible");
        }

        // Check for potential issues
        if (data.Transactions.Count > 50000)
        {
            warnings.Add($"Large import: {data.Transactions.Count:N0} transactions may take a while");
        }

        // Check category relationships
        var categoryIds = data.Categories.Select(c => c.OriginalId).ToHashSet();
        var orphanedCategories = data.Categories
            .Where(c => c.ParentId.HasValue && !categoryIds.Contains(c.ParentId.Value))
            .ToList();
        if (orphanedCategories.Count > 0)
        {
            warnings.Add($"{orphanedCategories.Count} categories have missing parent references");
        }

        // Check account references in transactions
        var accountIds = data.Accounts.Select(a => a.OriginalId).ToHashSet();
        var orphanedTransactions = data.Transactions
            .Where(t => !accountIds.Contains(t.OriginalAccountId))
            .ToList();
        if (orphanedTransactions.Count > 0)
        {
            warnings.Add($"{orphanedTransactions.Count} transactions reference unknown accounts");
        }

        var sessionId = Guid.CreateVersion7();
        var session = new JsonImportSession(sessionId, data, DateTime.UtcNow);
        ImportSessions[sessionId] = session;

        logger.LogInformation(
            "Created import session {SessionId}: {TransactionCount} transactions, {Warnings} warnings",
            sessionId, data.Transactions.Count, warnings.Count);

        var preview = new JsonImportPreviewResponse(
            data.Profile.Name,
            data.Profile.Type,
            data.Accounts.Count,
            data.Categories.Count,
            data.Rules.Count,
            data.ImportFormats.Count,
            data.Transactions.Count,
            warnings.ToArray());

        return (preview, sessionId);
    }

    public async Task<JsonImportResult> ConfirmJsonImportAsync(
        Guid sessionId,
        Guid userId,
        JsonImportConfirmRequest options,
        CancellationToken ct = default)
    {
        if (!ImportSessions.TryRemove(sessionId, out var session))
        {
            throw new InvalidOperationException($"Import session {sessionId} not found or expired");
        }

        var data = session.Data;

        // ID remapping dictionaries
        var accountIdMap = new Dictionary<Guid, Guid>();
        var categoryIdMap = new Dictionary<Guid, Guid>();

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Create Profile
            var profile = new Profile
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Name = options.ProfileName,
                Type = data.Profile.Type
            };
            db.Profiles.Add(profile);
            await db.SaveChangesAsync(ct);

            // 2. Create Categories (parents first, then children)
            var parentCategories = data.Categories.Where(c => c.ParentId == null).ToList();
            var childCategories = data.Categories.Where(c => c.ParentId != null).ToList();

            foreach (var c in parentCategories)
            {
                var category = new Category
                {
                    Id = Guid.CreateVersion7(),
                    ProfileId = profile.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Color = c.Color,
                    SortOrder = c.SortOrder,
                    ParentId = null
                };
                categoryIdMap[c.OriginalId] = category.Id;
                db.Categories.Add(category);
            }
            await db.SaveChangesAsync(ct);

            foreach (var c in childCategories)
            {
                var parentId = c.ParentId.HasValue && categoryIdMap.TryGetValue(c.ParentId.Value, out var mappedParentId)
                    ? mappedParentId
                    : (Guid?)null;

                var category = new Category
                {
                    Id = Guid.CreateVersion7(),
                    ProfileId = profile.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Color = c.Color,
                    SortOrder = c.SortOrder,
                    ParentId = parentId
                };
                categoryIdMap[c.OriginalId] = category.Id;
                db.Categories.Add(category);
            }
            await db.SaveChangesAsync(ct);

            // 3. Create Accounts
            foreach (var a in data.Accounts)
            {
                var account = new Account
                {
                    Id = Guid.CreateVersion7(),
                    ProfileId = profile.Id,
                    Name = a.Name,
                    BankName = a.BankName,
                    Currency = a.Currency
                };
                accountIdMap[a.OriginalId] = account.Id;
                db.Accounts.Add(account);
            }
            await db.SaveChangesAsync(ct);

            // 4. Create Import Formats (if requested)
            var formatsCreated = 0;
            if (options.ImportFormats)
            {
                foreach (var f in data.ImportFormats)
                {
                    var format = new ImportFormat
                    {
                        Id = Guid.CreateVersion7(),
                        ProfileId = profile.Id,
                        Name = f.Name,
                        BankName = f.BankName,
                        Mapping = f.Mapping
                    };
                    db.ImportFormats.Add(format);
                    formatsCreated++;
                }
                await db.SaveChangesAsync(ct);
            }

            // 5. Create Rules (if requested)
            var rulesCreated = 0;
            if (options.ImportRules)
            {
                foreach (var r in data.Rules)
                {
                    var rule = new CategorizationRule
                    {
                        Id = Guid.CreateVersion7(),
                        ProfileId = profile.Id,
                        Name = r.Name,
                        Priority = r.Priority,
                        RuleToml = r.RuleToml,
                        IsActive = r.IsActive
                    };
                    db.CategorizationRules.Add(rule);
                    rulesCreated++;
                }
                await db.SaveChangesAsync(ct);
            }

            // 6. Create Transactions (batch for performance)
            const int batchSize = 1000;
            var transactionsCreated = 0;

            for (var i = 0; i < data.Transactions.Count; i += batchSize)
            {
                var batch = data.Transactions.Skip(i).Take(batchSize);

                foreach (var t in batch)
                {
                    if (!accountIdMap.TryGetValue(t.OriginalAccountId, out var accountId))
                    {
                        logger.LogWarning("Skipping transaction with unknown account {AccountId}", t.OriginalAccountId);
                        continue;
                    }

                    var categoryId = t.OriginalCategoryId.HasValue &&
                                     categoryIdMap.TryGetValue(t.OriginalCategoryId.Value, out var mappedCategoryId)
                        ? mappedCategoryId
                        : (Guid?)null;

                    var txn = new Transaction
                    {
                        Id = Guid.CreateVersion7(),
                        AccountId = accountId,
                        CategoryId = categoryId,
                        Date = t.Date,
                        Amount = t.Amount,
                        Description = t.Description,
                        Notes = t.Notes,
                        Tags = t.Tags
                    };
                    db.Transactions.Add(txn);
                    transactionsCreated++;
                }

                await db.SaveChangesAsync(ct);
            }

            await transaction.CommitAsync(ct);

            logger.LogInformation(
                "Completed import for profile {ProfileId}: {Accounts} accounts, {Categories} categories, {Transactions} transactions",
                profile.Id, data.Accounts.Count, data.Categories.Count, transactionsCreated);

            return new JsonImportResult(
                profile.Id,
                data.Accounts.Count,
                data.Categories.Count,
                rulesCreated,
                formatsCreated,
                transactionsCreated);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Failed to import profile, rolled back transaction");
            throw;
        }
    }

    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
