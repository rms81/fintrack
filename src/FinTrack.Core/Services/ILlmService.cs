using FinTrack.Core.Domain.ValueObjects;

namespace FinTrack.Core.Services;

public interface ILlmService
{
    Task<CsvFormatConfig> DetectCsvFormatAsync(
        string csvSample,
        CancellationToken ct = default);

    Task<string> TranslateToSqlAsync(
        string naturalLanguageQuery,
        string schemaContext,
        CancellationToken ct = default);
}
