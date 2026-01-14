using FinTrack.Core.Domain.ValueObjects;
using FinTrack.Core.Services;

namespace FinTrack.Infrastructure.Services;

/// <summary>
/// Stub implementation used when no LLM API key is configured.
/// Throws InvalidOperationException if any method is called.
/// </summary>
public sealed class StubLlmService : ILlmService
{
    public Task<CsvFormatConfig> DetectCsvFormatAsync(string csvSample, CancellationToken ct = default)
        => throw new InvalidOperationException("LLM service is not configured. Set Llm:ApiKey in configuration.");

    public Task<string> TranslateToSqlAsync(string naturalLanguageQuery, string schemaContext, CancellationToken ct = default)
        => throw new InvalidOperationException("LLM service is not configured. Set Llm:ApiKey in configuration.");
}
