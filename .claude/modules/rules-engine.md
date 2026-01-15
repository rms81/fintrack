# Rules Engine Module

> **Phase:** 2 (Import & Rules)
> **Status:** Planned

## Overview
TOML-based deterministic rules for automatic transaction categorization.

## User Stories

| ID | As a... | I want to... | So that... |
|----|---------|--------------|------------|
| US-R1 | User | create categorization rules | transactions are automatically categorized |
| US-R2 | User | match rules by description text | I can target specific merchants or keywords |
| US-R3 | User | match rules by amount ranges | I can categorize based on transaction size |
| US-R4 | User | set rule priority | I can control which rule applies when multiple match |
| US-R5 | User | test rules against sample transactions | I can verify rules work before saving |
| US-R6 | User | apply rules to existing uncategorized transactions | historical data gets categorized |
| US-R7 | User | enable/disable rules without deleting | I can temporarily turn off rules |
| US-R8 | User | use rule templates | I can quickly add common categorization patterns |
| US-R9 | User | add tags via rules | I can automatically tag transactions for grouping |

## Why TOML?

- Human-readable and editable
- No LLM costs for categorization
- Deterministic and predictable
- Easy to version control
- Portable between profiles

## Rule Format

```toml
[[rules]]
name = "Uber Rides"
priority = 10
category = "Transportation"
match.description = { contains = ["UBER", "uber"] }

[[rules]]
name = "Netflix Subscription"
priority = 20
category = "Entertainment"
match.description = { equals = "NETFLIX.COM" }
match.amount = { range = [-20, -10] }

[[rules]]
name = "Grocery Stores"
priority = 30
category = "Food & Dining"
match.description = { regex = "(?i)(lidl|aldi|continente|pingo doce)" }

[[rules]]
name = "Large Expenses"
priority = 100
tags = ["review", "large"]
match.amount = { less_than = -500 }
```

## Match Conditions

<!-- TODO: Phase 2 - Implement matchers -->

### Description Matchers
- `contains` - Array of substrings (OR logic)
- `equals` - Exact match
- `starts_with` - Prefix match
- `ends_with` - Suffix match
- `regex` - Regular expression

### Amount Matchers
- `equals` - Exact amount
- `greater_than` - Minimum amount
- `less_than` - Maximum amount
- `range` - [min, max] inclusive

### Date Matchers
- `day_of_week` - [1-7] (Monday=1)
- `day_of_month` - [1-31]

## Rule Actions

- `category` - Set category by name
- `tags` - Add tags (array)
- `skip` - Skip further rules (boolean)

## Domain Model

```csharp
public class CategorizationRule : BaseEntity
{
    public Guid ProfileId { get; init; }
    public required string Name { get; set; }
    public int Priority { get; set; }
    public required string RuleToml { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public Profile Profile { get; init; } = null!;
}
```

## Service Interface

```csharp
public interface IRulesEngine
{
    Task<CategoryMatch?> MatchAsync(
        Transaction transaction,
        IEnumerable<CategorizationRule> rules,
        CancellationToken ct = default);
        
    Task ApplyRulesAsync(
        IEnumerable<Transaction> transactions,
        Guid profileId,
        CancellationToken ct = default);
}

public record CategoryMatch(
    Guid? CategoryId,
    string[] Tags,
    string MatchedRuleName
);
```

## Endpoints

<!-- TODO: Phase 2 -->

### GET /api/profiles/{profileId}/rules
List categorization rules.

### POST /api/profiles/{profileId}/rules
Create new rule.

### PUT /api/rules/{id}
Update rule.

### POST /api/profiles/{profileId}/rules/test
Test rules against sample transactions.

### POST /api/profiles/{profileId}/rules/apply
Re-apply rules to existing uncategorized transactions.

## React Components

<!-- TODO: Phase 2 -->

- Rule list with priority ordering
- TOML editor with syntax highlighting
- Rule tester UI
