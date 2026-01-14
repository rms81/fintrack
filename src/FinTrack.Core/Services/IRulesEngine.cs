using FinTrack.Core.Domain.Entities;

namespace FinTrack.Core.Services;

public interface IRulesEngine
{
    Task<CategoryMatch?> MatchAsync(
        Transaction transaction,
        IEnumerable<CategorizationRule> rules,
        CancellationToken ct = default);

    Task<int> ApplyRulesAsync(
        IEnumerable<Transaction> transactions,
        IEnumerable<CategorizationRule> rules,
        Func<string, Guid?> categoryLookup,
        CancellationToken ct = default);

    RuleValidationResult ValidateRule(string toml);
}

public record CategoryMatch(
    Guid? CategoryId,
    string? CategoryName,
    string[] Tags,
    string MatchedRuleName);

public record RuleValidationResult(
    bool IsValid,
    string? ErrorMessage = null);
