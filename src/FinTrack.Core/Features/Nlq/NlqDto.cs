namespace FinTrack.Core.Features.Nlq;

public record NlqRequest(string Question);

public record NlqResponse(
    string Question,
    string? GeneratedSql,
    NlqResultType ResultType,
    object? Data,
    string? Explanation,
    string? ChartType,
    string? ErrorMessage);

public enum NlqResultType
{
    Scalar,
    Table,
    Chart,
    Error
}

public record NlqLlmResponse
{
    public string Sql { get; init; } = "";
    public string ResultType { get; init; } = "table";
    public string? ChartType { get; init; }
    public string? Explanation { get; init; }
}
