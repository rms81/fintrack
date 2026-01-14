namespace FinTrack.Core.Domain.ValueObjects;

public record CsvFormatConfig
{
    public string Delimiter { get; init; } = ",";
    public bool HasHeader { get; init; } = true;
    public int DateColumn { get; init; }
    public string DateFormat { get; init; } = "yyyy-MM-dd";
    public int DescriptionColumn { get; init; }
    public string AmountType { get; init; } = "signed";
    public int? AmountColumn { get; init; }
    public int? DebitColumn { get; init; }
    public int? CreditColumn { get; init; }
    public int? BalanceColumn { get; init; }
}
