using FinTrack.Core.Domain.Enums;
using FinTrack.Core.Domain.ValueObjects;

namespace FinTrack.Core.Features.Export;

// JSON Export - Full profile backup
public record ProfileExportData
{
    public string Version { get; init; } = "1.0";
    public required DateTime ExportedAt { get; init; }
    public required ProfileExportInfo Profile { get; init; }
    public required IReadOnlyList<AccountExportInfo> Accounts { get; init; }
    public required IReadOnlyList<CategoryExportInfo> Categories { get; init; }
    public required IReadOnlyList<RuleExportInfo> Rules { get; init; }
    public required IReadOnlyList<ImportFormatExportInfo> ImportFormats { get; init; }
    public required IReadOnlyList<TransactionExportInfo> Transactions { get; init; }
}

public record ProfileExportInfo(string Name, ProfileType Type);

public record AccountExportInfo(
    Guid OriginalId,
    string Name,
    string? BankName,
    string Currency);

public record CategoryExportInfo(
    Guid OriginalId,
    string Name,
    string Icon,
    string Color,
    int SortOrder,
    Guid? ParentId);

public record RuleExportInfo(
    string Name,
    int Priority,
    string RuleToml,
    bool IsActive);

public record ImportFormatExportInfo(
    string Name,
    string? BankName,
    CsvFormatConfig Mapping);

public record TransactionExportInfo(
    Guid OriginalAccountId,
    DateOnly Date,
    decimal Amount,
    string Description,
    string? Notes,
    string[] Tags,
    Guid? OriginalCategoryId);

// Export options
public record JsonExportOptions(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    bool IncludeRules = true,
    bool IncludeFormats = true);

public record CsvExportOptions(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? AccountId = null,
    Guid? CategoryId = null);

// JSON Import - Preview and confirm
public record JsonImportPreviewResponse(
    string ProfileName,
    ProfileType ProfileType,
    int AccountCount,
    int CategoryCount,
    int RuleCount,
    int ImportFormatCount,
    int TransactionCount,
    string[] Warnings);

public record JsonImportConfirmRequest(
    Guid SessionId,
    string ProfileName,
    bool ImportRules = true,
    bool ImportFormats = true);

public record JsonImportResult(
    Guid NewProfileId,
    int AccountsCreated,
    int CategoriesCreated,
    int RulesCreated,
    int FormatsCreated,
    int TransactionsCreated);

// Import session for tracking parsed data
public record JsonImportSession(
    Guid SessionId,
    ProfileExportData Data,
    DateTime CreatedAt);
