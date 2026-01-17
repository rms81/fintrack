using FinTrack.Core.Features.Export;

namespace FinTrack.Core.Services;

public interface IExportService
{
    Task<ProfileExportData> ExportProfileJsonAsync(
        Guid profileId,
        JsonExportOptions options,
        CancellationToken ct = default);

    Task<byte[]> ExportTransactionsCsvAsync(
        Guid profileId,
        CsvExportOptions options,
        CancellationToken ct = default);

    Task<(JsonImportPreviewResponse Preview, Guid SessionId)> PreviewJsonImportAsync(
        Stream jsonStream,
        CancellationToken ct = default);

    Task<JsonImportResult> ConfirmJsonImportAsync(
        Guid sessionId,
        Guid userId,
        JsonImportConfirmRequest options,
        CancellationToken ct = default);
}
