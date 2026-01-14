namespace FinTrack.Core.Features.Transactions;

public record TransactionDto(
    Guid Id,
    Guid AccountId,
    Guid? CategoryId,
    string? CategoryName,
    DateOnly Date,
    decimal Amount,
    string Description,
    string? Notes,
    string[] Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record UpdateTransactionRequest(
    Guid? CategoryId,
    string? Notes,
    string[]? Tags);

public record TransactionFilter(
    Guid ProfileId,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    string? Search = null,
    string[]? Tags = null,
    bool? Uncategorized = null,
    int Page = 1,
    int PageSize = 20);

public record TransactionPage(
    IReadOnlyList<TransactionDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record GetTransactions(TransactionFilter Filter);
public record GetTransaction(Guid Id);
public record UpdateTransaction(Guid Id, Guid? CategoryId, string? Notes, string[]? Tags);
public record DeleteTransaction(Guid Id);
public record DeleteTransactionResult(bool Success);
