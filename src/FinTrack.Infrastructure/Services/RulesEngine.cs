using System.Text.RegularExpressions;
using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Services;
using Microsoft.Extensions.Logging;
using Tomlyn;
using Tomlyn.Model;

namespace FinTrack.Infrastructure.Services;

public class RulesEngine(ILogger<RulesEngine> logger) : IRulesEngine
{
    public Task<CategoryMatch?> MatchAsync(
        Transaction transaction,
        IEnumerable<CategorizationRule> rules,
        CancellationToken ct = default)
    {
        var sortedRules = rules
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToList();

        foreach (var rule in sortedRules)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var parsed = ParseRule(rule.RuleToml);
                if (parsed is null) continue;

                if (MatchesRule(transaction, parsed))
                {
                    return Task.FromResult<CategoryMatch?>(new CategoryMatch(
                        null, // CategoryId resolved by caller
                        parsed.Category,
                        parsed.Tags ?? [],
                        rule.Name));
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to evaluate rule {RuleName}", rule.Name);
            }
        }

        return Task.FromResult<CategoryMatch?>(null);
    }

    public async Task<int> ApplyRulesAsync(
        IEnumerable<Transaction> transactions,
        IEnumerable<CategorizationRule> rules,
        Func<string, Guid?> categoryLookup,
        CancellationToken ct = default)
    {
        var rulesList = rules.ToList();
        var updated = 0;

        foreach (var transaction in transactions)
        {
            ct.ThrowIfCancellationRequested();

            var match = await MatchAsync(transaction, rulesList, ct);
            if (match is not null)
            {
                if (!string.IsNullOrEmpty(match.CategoryName))
                {
                    var categoryId = categoryLookup(match.CategoryName);
                    if (categoryId.HasValue)
                    {
                        transaction.CategoryId = categoryId;
                    }
                }

                if (match.Tags.Length > 0)
                {
                    transaction.Tags = transaction.Tags
                        .Union(match.Tags)
                        .Distinct()
                        .ToArray();
                }

                updated++;
            }
        }

        return updated;
    }

    public RuleValidationResult ValidateRule(string toml)
    {
        try
        {
            var parsed = ParseRule(toml);
            if (parsed is null)
            {
                return new RuleValidationResult(false, "Failed to parse rule");
            }

            if (string.IsNullOrEmpty(parsed.Name))
            {
                return new RuleValidationResult(false, "Rule must have a name");
            }

            if (parsed.Match is null || (!parsed.Match.Description.HasValue && !parsed.Match.Amount.HasValue && !parsed.Match.Date.HasValue))
            {
                return new RuleValidationResult(false, "Rule must have at least one match condition");
            }

            return new RuleValidationResult(true);
        }
        catch (Exception ex)
        {
            return new RuleValidationResult(false, ex.Message);
        }
    }

    private static ParsedRule? ParseRule(string toml)
    {
        var doc = Toml.ToModel(toml);

        // Handle array of rules format [[rules]]
        if (doc.TryGetValue("rules", out var rulesObj) && rulesObj is TomlTableArray rulesArray)
        {
            if (rulesArray.Count > 0)
            {
                return ParseSingleRule(rulesArray[0]);
            }
        }

        // Handle single rule format [rule] or root level
        return ParseSingleRule(doc);
    }

    private static ParsedRule? ParseSingleRule(TomlTable table)
    {
        var rule = new ParsedRule
        {
            Name = table.TryGetValue("name", out var name) ? name?.ToString() : null,
            Priority = table.TryGetValue("priority", out var priority) ? Convert.ToInt32(priority) : 0,
            Category = table.TryGetValue("category", out var category) ? category?.ToString() : null,
            Tags = table.TryGetValue("tags", out var tags) && tags is TomlArray tagsArray
                ? tagsArray.Select(t => t?.ToString() ?? "").ToArray()
                : null,
            Skip = table.TryGetValue("skip", out var skip) && skip is bool skipVal && skipVal
        };

        if (table.TryGetValue("match", out var matchObj) && matchObj is TomlTable matchTable)
        {
            rule.Match = ParseMatchConditions(matchTable);
        }

        return rule;
    }

    private static MatchConditions ParseMatchConditions(TomlTable matchTable)
    {
        var conditions = new MatchConditions();

        if (matchTable.TryGetValue("description", out var descObj) && descObj is TomlTable descTable)
        {
            conditions.Description = ParseDescriptionMatcher(descTable);
        }

        if (matchTable.TryGetValue("amount", out var amountObj) && amountObj is TomlTable amountTable)
        {
            conditions.Amount = ParseAmountMatcher(amountTable);
        }

        if (matchTable.TryGetValue("date", out var dateObj) && dateObj is TomlTable dateTable)
        {
            conditions.Date = ParseDateMatcher(dateTable);
        }

        return conditions;
    }

    private static DescriptionMatcher? ParseDescriptionMatcher(TomlTable table)
    {
        var matcher = new DescriptionMatcher();

        if (table.TryGetValue("contains", out var contains) && contains is TomlArray containsArr)
        {
            matcher.Contains = containsArr.Select(x => x?.ToString() ?? "").ToArray();
        }

        if (table.TryGetValue("equals", out var equals))
        {
            matcher.Equals = equals?.ToString();
        }

        if (table.TryGetValue("starts_with", out var startsWith))
        {
            matcher.StartsWith = startsWith?.ToString();
        }

        if (table.TryGetValue("ends_with", out var endsWith))
        {
            matcher.EndsWith = endsWith?.ToString();
        }

        if (table.TryGetValue("regex", out var regex))
        {
            matcher.Regex = regex?.ToString();
        }

        return matcher.HasConditions ? matcher : null;
    }

    private static AmountMatcher? ParseAmountMatcher(TomlTable table)
    {
        var matcher = new AmountMatcher();

        if (table.TryGetValue("equals", out var equals))
        {
            matcher.Equals = Convert.ToDecimal(equals);
        }

        if (table.TryGetValue("greater_than", out var greaterThan))
        {
            matcher.GreaterThan = Convert.ToDecimal(greaterThan);
        }

        if (table.TryGetValue("less_than", out var lessThan))
        {
            matcher.LessThan = Convert.ToDecimal(lessThan);
        }

        if (table.TryGetValue("range", out var range) && range is TomlArray rangeArr && rangeArr.Count == 2)
        {
            matcher.RangeMin = Convert.ToDecimal(rangeArr[0]);
            matcher.RangeMax = Convert.ToDecimal(rangeArr[1]);
        }

        return matcher.HasConditions ? matcher : null;
    }

    private static DateMatcher? ParseDateMatcher(TomlTable table)
    {
        var matcher = new DateMatcher();

        if (table.TryGetValue("day_of_week", out var dayOfWeek))
        {
            matcher.DayOfWeek = Convert.ToInt32(dayOfWeek);
        }

        if (table.TryGetValue("day_of_month", out var dayOfMonth))
        {
            matcher.DayOfMonth = Convert.ToInt32(dayOfMonth);
        }

        return matcher.HasConditions ? matcher : null;
    }

    private static bool MatchesRule(Transaction transaction, ParsedRule rule)
    {
        if (rule.Match is null) return false;

        // All conditions must match (AND logic)
        if (rule.Match.Description.HasValue && !MatchesDescription(transaction.Description, rule.Match.Description.Value))
            return false;

        if (rule.Match.Amount.HasValue && !MatchesAmount(transaction.Amount, rule.Match.Amount.Value))
            return false;

        if (rule.Match.Date.HasValue && !MatchesDate(transaction.Date, rule.Match.Date.Value))
            return false;

        return true;
    }

    private static bool MatchesDescription(string description, DescriptionMatcher matcher)
    {
        // Contains: any substring matches (OR logic)
        if (matcher.Contains?.Length > 0)
        {
            if (!matcher.Contains.Any(c => description.Contains(c, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        if (!string.IsNullOrEmpty(matcher.Equals))
        {
            if (!description.Equals(matcher.Equals, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrEmpty(matcher.StartsWith))
        {
            if (!description.StartsWith(matcher.StartsWith, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrEmpty(matcher.EndsWith))
        {
            if (!description.EndsWith(matcher.EndsWith, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrEmpty(matcher.Regex))
        {
            try
            {
                if (!Regex.IsMatch(description, matcher.Regex, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                    return false;
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesAmount(decimal amount, AmountMatcher matcher)
    {
        if (matcher.Equals.HasValue && amount != matcher.Equals.Value)
            return false;

        if (matcher.GreaterThan.HasValue && amount <= matcher.GreaterThan.Value)
            return false;

        if (matcher.LessThan.HasValue && amount >= matcher.LessThan.Value)
            return false;

        if (matcher.RangeMin.HasValue && matcher.RangeMax.HasValue)
        {
            if (amount < matcher.RangeMin.Value || amount > matcher.RangeMax.Value)
                return false;
        }

        return true;
    }

    private static bool MatchesDate(DateOnly date, DateMatcher matcher)
    {
        if (matcher.DayOfWeek.HasValue)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            // Convert to ISO format (Monday=1, Sunday=7)
            if (dayOfWeek == 0) dayOfWeek = 7;
            if (dayOfWeek != matcher.DayOfWeek.Value)
                return false;
        }

        if (matcher.DayOfMonth.HasValue && date.Day != matcher.DayOfMonth.Value)
            return false;

        return true;
    }

    // Internal models for parsed rules
    private class ParsedRule
    {
        public string? Name { get; set; }
        public int Priority { get; set; }
        public string? Category { get; set; }
        public string[]? Tags { get; set; }
        public bool Skip { get; set; }
        public MatchConditions? Match { get; set; }
    }

    private class MatchConditions
    {
        public DescriptionMatcher? Description { get; set; }
        public AmountMatcher? Amount { get; set; }
        public DateMatcher? Date { get; set; }
    }

    private struct DescriptionMatcher
    {
        public string[]? Contains { get; set; }
        public string? Equals { get; set; }
        public string? StartsWith { get; set; }
        public string? EndsWith { get; set; }
        public string? Regex { get; set; }

        public readonly bool HasConditions =>
            Contains?.Length > 0 ||
            !string.IsNullOrEmpty(Equals) ||
            !string.IsNullOrEmpty(StartsWith) ||
            !string.IsNullOrEmpty(EndsWith) ||
            !string.IsNullOrEmpty(Regex);
    }

    private struct AmountMatcher
    {
        public decimal? Equals { get; set; }
        public decimal? GreaterThan { get; set; }
        public decimal? LessThan { get; set; }
        public decimal? RangeMin { get; set; }
        public decimal? RangeMax { get; set; }

        public readonly bool HasConditions =>
            Equals.HasValue ||
            GreaterThan.HasValue ||
            LessThan.HasValue ||
            (RangeMin.HasValue && RangeMax.HasValue);
    }

    private struct DateMatcher
    {
        public int? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }

        public readonly bool HasConditions => DayOfWeek.HasValue || DayOfMonth.HasValue;
    }
}
