using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Domain.ValueObjects;
using FinTrack.Core.Features.Import;
using FinTrack.Core.Services;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Services;

public class CsvImportService(
    ILlmService? llmService,
    ILogger<CsvImportService> logger) : IImportService
{
    public async Task<(CsvFormatConfig Format, string[] SampleRows, int RowCount)> AnalyzeCsvAsync(
        byte[] csvData,
        CancellationToken ct = default)
    {
        var content = Encoding.UTF8.GetString(csvData);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var sampleRows = lines.Take(6).ToArray();

        CsvFormatConfig format;

        if (llmService is not null)
        {
            try
            {
                var sample = string.Join('\n', sampleRows);
                format = await llmService.DetectCsvFormatAsync(sample, ct);
                logger.LogInformation("LLM detected format: delimiter={Delimiter}, dateFormat={DateFormat}",
                    format.Delimiter, format.DateFormat);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "LLM format detection failed, using heuristics");
                format = DetectFormatHeuristically(sampleRows);
            }
        }
        else
        {
            logger.LogInformation("LLM service not available, using heuristic detection");
            format = DetectFormatHeuristically(sampleRows);
        }

        return (format, sampleRows, lines.Length);
    }

    public IReadOnlyList<TransactionPreview> ParseTransactions(
        byte[] csvData,
        CsvFormatConfig format,
        ISet<string> existingHashes)
    {
        var content = Encoding.UTF8.GetString(csvData);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var startIndex = format.HasHeader ? 1 : 0;

        var transactions = new List<TransactionPreview>();

        for (var i = startIndex; i < lines.Length; i++)
        {
            try
            {
                var columns = ParseCsvLine(lines[i], format.Delimiter);
                if (columns.Length == 0) continue;

                var date = ParseDate(columns, format);
                var description = columns[format.DescriptionColumn].Trim();
                var amount = ParseAmount(columns, format);

                var hash = ComputeDuplicateHash(date, amount, description);
                var isDuplicate = existingHashes.Contains(hash);

                transactions.Add(new TransactionPreview(date, description, amount, isDuplicate));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to parse line {LineNumber}", i + 1);
            }
        }

        return transactions;
    }

    public IReadOnlyList<Transaction> CreateTransactions(
        byte[] csvData,
        CsvFormatConfig format,
        Guid accountId,
        ISet<string> existingHashes,
        bool skipDuplicates)
    {
        var content = Encoding.UTF8.GetString(csvData);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var startIndex = format.HasHeader ? 1 : 0;

        var transactions = new List<Transaction>();

        for (var i = startIndex; i < lines.Length; i++)
        {
            try
            {
                var columns = ParseCsvLine(lines[i], format.Delimiter);
                if (columns.Length == 0) continue;

                var date = ParseDate(columns, format);
                var description = columns[format.DescriptionColumn].Trim();
                var amount = ParseAmount(columns, format);

                var hash = ComputeDuplicateHash(date, amount, description);

                if (skipDuplicates && existingHashes.Contains(hash))
                {
                    continue;
                }

                transactions.Add(new Transaction
                {
                    AccountId = accountId,
                    Date = date,
                    Description = description,
                    Amount = amount,
                    DuplicateHash = hash
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to create transaction from line {LineNumber}", i + 1);
            }
        }

        return transactions;
    }

    public string ComputeDuplicateHash(DateOnly date, decimal amount, string description)
    {
        var input = $"{date:yyyy-MM-dd}|{amount:F2}|{description.ToUpperInvariant()}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16];
    }

    private static CsvFormatConfig DetectFormatHeuristically(string[] sampleRows)
    {
        if (sampleRows.Length == 0)
        {
            return new CsvFormatConfig();
        }

        // Detect delimiter
        var firstLine = sampleRows[0];
        var delimiter = ",";
        if (firstLine.Count(c => c == ';') > firstLine.Count(c => c == ','))
        {
            delimiter = ";";
        }
        else if (firstLine.Count(c => c == '\t') > firstLine.Count(c => c == ','))
        {
            delimiter = "\t";
        }

        var columns = ParseCsvLine(firstLine, delimiter);

        // Check if first row is header (contains non-numeric text in columns that might be amounts)
        var hasHeader = columns.Any(c =>
            !decimal.TryParse(c.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
            !DateTime.TryParse(c, out _));

        // Try to detect date column and format
        var dateColumn = 0;
        var dateFormat = "yyyy-MM-dd";
        var testRow = sampleRows.Length > 1 ? sampleRows[1] : firstLine;
        var testColumns = ParseCsvLine(testRow, delimiter);

        for (var i = 0; i < testColumns.Length; i++)
        {
            var col = testColumns[i].Trim();
            if (TryDetectDateFormat(col, out var detectedFormat))
            {
                dateColumn = i;
                dateFormat = detectedFormat;
                break;
            }
        }

        // Try to find amount column (likely has decimal numbers)
        var amountColumn = 0;
        for (var i = 0; i < testColumns.Length; i++)
        {
            var col = testColumns[i].Trim().Replace(",", ".");
            if (decimal.TryParse(col, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                amountColumn = i;
                if (i != dateColumn)
                {
                    break;
                }
            }
        }

        // Description is typically the longest text column
        var descriptionColumn = 0;
        var maxLength = 0;
        for (var i = 0; i < testColumns.Length; i++)
        {
            if (i == dateColumn || i == amountColumn) continue;
            if (testColumns[i].Length > maxLength)
            {
                maxLength = testColumns[i].Length;
                descriptionColumn = i;
            }
        }

        return new CsvFormatConfig
        {
            Delimiter = delimiter,
            HasHeader = hasHeader,
            DateColumn = dateColumn,
            DateFormat = dateFormat,
            DescriptionColumn = descriptionColumn,
            AmountType = "signed",
            AmountColumn = amountColumn
        };
    }

    private static bool TryDetectDateFormat(string value, out string format)
    {
        var formats = new[]
        {
            ("dd/MM/yyyy", @"^\d{2}/\d{2}/\d{4}$"),
            ("MM/dd/yyyy", @"^\d{2}/\d{2}/\d{4}$"),
            ("yyyy-MM-dd", @"^\d{4}-\d{2}-\d{2}$"),
            ("dd-MM-yyyy", @"^\d{2}-\d{2}-\d{4}$"),
            ("dd.MM.yyyy", @"^\d{2}\.\d{2}\.\d{4}$"),
        };

        foreach (var (fmt, pattern) in formats)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
            {
                if (DateTime.TryParseExact(value, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    format = fmt;
                    return true;
                }
            }
        }

        format = "yyyy-MM-dd";
        return false;
    }

    private static string[] ParseCsvLine(string line, string delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        var delimChar = delimiter[0];

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimChar && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }

    private static DateOnly ParseDate(string[] columns, CsvFormatConfig format)
    {
        var dateStr = columns[format.DateColumn].Trim();
        if (DateTime.TryParseExact(dateStr, format.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return DateOnly.FromDateTime(date);
        }

        // Fallback to generic parsing
        if (DateTime.TryParse(dateStr, out date))
        {
            return DateOnly.FromDateTime(date);
        }

        throw new FormatException($"Could not parse date: {dateStr}");
    }

    private static decimal ParseAmount(string[] columns, CsvFormatConfig format)
    {
        if (format.AmountType == "split")
        {
            var debit = 0m;
            var credit = 0m;

            if (format.DebitColumn.HasValue)
            {
                var debitStr = columns[format.DebitColumn.Value].Trim().Replace(",", ".");
                if (!string.IsNullOrEmpty(debitStr))
                {
                    decimal.TryParse(debitStr, NumberStyles.Any, CultureInfo.InvariantCulture, out debit);
                }
            }

            if (format.CreditColumn.HasValue)
            {
                var creditStr = columns[format.CreditColumn.Value].Trim().Replace(",", ".");
                if (!string.IsNullOrEmpty(creditStr))
                {
                    decimal.TryParse(creditStr, NumberStyles.Any, CultureInfo.InvariantCulture, out credit);
                }
            }

            return credit - debit;
        }

        if (format.AmountColumn.HasValue)
        {
            var amountStr = columns[format.AmountColumn.Value].Trim().Replace(",", ".");
            if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                return amount;
            }
        }

        throw new FormatException("Could not parse amount");
    }
}
