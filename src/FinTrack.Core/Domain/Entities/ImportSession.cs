using FinTrack.Core.Domain.Enums;
using FinTrack.Core.Domain.ValueObjects;

namespace FinTrack.Core.Domain.Entities;

public class ImportSession : Entity
{
    public required Guid AccountId { get; init; }
    public required string Filename { get; init; }
    public int RowCount { get; set; }
    public ImportStatus Status { get; set; } = ImportStatus.Pending;
    public string? ErrorMessage { get; set; }
    public CsvFormatConfig? FormatConfig { get; set; }
    public byte[]? CsvData { get; set; }

    public Account? Account { get; init; }
}
