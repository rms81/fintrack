using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Domain.ValueObjects;
using FinTrack.Core.Features.Import;

namespace FinTrack.Core.Services;

public interface IImportService
{
    Task<(CsvFormatConfig Format, string[] SampleRows, int RowCount)> AnalyzeCsvAsync(
        byte[] csvData,
        CancellationToken ct = default);

    IReadOnlyList<TransactionPreview> ParseTransactions(
        byte[] csvData,
        CsvFormatConfig format,
        ISet<string> existingHashes);

    IReadOnlyList<Transaction> CreateTransactions(
        byte[] csvData,
        CsvFormatConfig format,
        Guid accountId,
        ISet<string> existingHashes,
        bool skipDuplicates);

    string ComputeDuplicateHash(DateOnly date, decimal amount, string description);
}
