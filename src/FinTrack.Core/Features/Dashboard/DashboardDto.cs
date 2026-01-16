namespace FinTrack.Core.Features.Dashboard;

public record DashboardSummaryDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetBalance,
    int TransactionCount,
    int UncategorizedCount,
    string? TopCategoryName,
    decimal? TopCategoryAmount,
    decimal? PreviousPeriodExpenses,
    decimal? ExpenseChangePercentage);

public record CategorySpendingDto(
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    decimal Amount,
    decimal Percentage,
    int TransactionCount);

public record SpendingOverTimeDto(
    DateOnly Date,
    decimal Income,
    decimal Expenses,
    decimal Net);

public record TopMerchantDto(
    string Merchant,
    decimal TotalAmount,
    int TransactionCount,
    DateOnly LastTransactionDate,
    string? MostCommonCategory);
