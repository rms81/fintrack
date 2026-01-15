using FinTrack.Core.Domain.Enums;
using FinTrack.Core.Domain.ValueObjects;

namespace FinTrack.Core.Features.Import;

public record ImportSessionDto(
    Guid Id,
    Guid AccountId,
    string Filename,
    int RowCount,
    ImportStatus Status,
    string? ErrorMessage,
    CsvFormatConfig? FormatConfig,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record UploadResponse(
    Guid SessionId,
    string Filename,
    int RowCount,
    CsvFormatConfig DetectedFormat,
    string[] SampleRows);

public record PreviewRequest(CsvFormatConfig? FormatOverride = null);

public record PreviewResponse(
    Guid SessionId,
    IReadOnlyList<TransactionPreview> Transactions,
    int DuplicateCount);

public record TransactionPreview(
    DateOnly Date,
    string Description,
    decimal Amount,
    bool IsDuplicate);

public record ConfirmImportRequest(
    CsvFormatConfig? FormatOverride = null,
    bool SkipDuplicates = true);

public record ConfirmImportResponse(
    int ImportedCount,
    int SkippedDuplicates);

public record ImportFormatDto(
    Guid Id,
    string Name,
    string? BankName,
    CsvFormatConfig Mapping,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateImportFormatRequest(
    string Name,
    string? BankName,
    CsvFormatConfig Mapping);

public record UpdateImportFormatRequest(
    string Name,
    string? BankName,
    CsvFormatConfig Mapping);
