using FinTrack.Core.Domain.ValueObjects;

namespace FinTrack.Core.Domain.Entities;

public class ImportFormat : Entity
{
    public required Guid ProfileId { get; init; }
    public required string Name { get; set; }
    public string? BankName { get; set; }
    public required CsvFormatConfig Mapping { get; set; }

    public Profile? Profile { get; init; }
}
