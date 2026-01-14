namespace FinTrack.Core.Features.Rules;

public record RuleDto(
    Guid Id,
    string Name,
    int Priority,
    string RuleToml,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateRuleRequest(
    string Name,
    int Priority,
    string RuleToml,
    bool IsActive = true);

public record UpdateRuleRequest(
    string Name,
    int Priority,
    string RuleToml,
    bool IsActive);

public record TestRulesRequest(
    string Description,
    decimal Amount,
    DateOnly Date);

public record TestRulesResponse(
    string? MatchedRuleName,
    string? Category,
    string[] Tags);

public record ApplyRulesRequest(bool OnlyUncategorized = true);

public record ApplyRulesResponse(int TransactionsUpdated);
