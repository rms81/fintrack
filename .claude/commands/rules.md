# Rules Engine

Work with the TOML-based rules engine for transaction categorization.

## Arguments
- `$ARGUMENTS` - The action to perform (e.g., "add rule for Netflix", "test rule", "explain syntax")

## Instructions

The rules engine uses TOML format inspired by davidfowl/tally. Each rule has:
- A unique name (the TOML section header)
- A `match` expression
- A `category` (required)
- Optional `subcategory`
- Optional `tags` array

## TOML Rule Syntax

### Basic Rule
```toml
[netflix]
match = 'normalized("NETFLIX")'
category = "Subscriptions"
subcategory = "Streaming"
tags = ["entertainment", "recurring"]
```

### Match Functions

| Function | Description | Example |
|----------|-------------|---------|
| `normalized(pattern)` | Ignores spaces, punctuation, case | `normalized("NETFLIX")` |
| `contains(pattern)` | Substring match | `contains("RESTAURANT")` |
| `startswith(pattern)` | Prefix match | `startswith("UBER")` |
| `anyof(p1, p2, ...)` | Match any pattern | `anyof("AMAZON", "AMZN")` |
| `regex(pattern)` | Regular expression | `regex("UBER\\s(?!EATS)")` |

### Conditions

| Condition | Description | Example |
|-----------|-------------|---------|
| `amount > X` | Amount greater than | `amount > 100` |
| `amount < X` | Amount less than | `amount < 10` |
| `amount >= X` | Amount greater or equal | `amount >= 50` |
| `amount <= X` | Amount less or equal | `amount <= 1000` |

### Logical Operators

```toml
# AND - both conditions must match
[large_amazon]
match = 'anyof("AMAZON", "AMZN") and amount > 100'
category = "Shopping"

# OR - either condition matches
[food_delivery]
match = 'contains("UBER EATS") or contains("DOORDASH") or contains("GLOVO")'
category = "Food"
subcategory = "Delivery"
```

### Complex Examples

```toml
# Portuguese grocery stores
[groceries_pt]
match = 'anyof("CONTINENTE", "PINGO DOCE", "LIDL", "ALDI", "MERCADONA", "MINIPRECO")'
category = "Food"
subcategory = "Groceries"
tags = ["essential"]

# Uber rides (not Uber Eats)
[uber_rides]
match = 'regex("UBER\\s(?!EATS)")'
category = "Transport"
subcategory = "Rideshare"

# Small recurring subscriptions
[small_subscriptions]
match = 'amount < 20 and (contains("SUBSCRIPTION") or contains("MONTHLY"))'
category = "Subscriptions"
subcategory = "Other"
tags = ["recurring"]

# ATM withdrawals
[atm]
match = 'anyof("ATM", "MULTIBANCO", "CASH WITHDRAWAL", "LEVANTAMENTO")'
category = "Cash"
tags = ["atm"]

# Transfers between own accounts (to exclude from reports)
[transfers]
match = 'contains("TRANSFER") and contains("OWN ACCOUNT")'
category = "Transfer"
tags = ["internal", "exclude-reports"]
```

## Implementation

### Rule Parser (C#)
```csharp
public interface IMatchExpression
{
    bool Matches(TransactionContext ctx);
}

public record TransactionContext(
    string Description,
    string NormalizedDescription,
    decimal Amount
);

// Example implementations
public record NormalizedMatch(string Pattern) : IMatchExpression
{
    private static readonly Regex NonAlphanumeric = new(@"[\s\-_.,]", RegexOptions.Compiled);
    
    public bool Matches(TransactionContext ctx)
    {
        var normalizedPattern = Normalize(Pattern);
        var normalizedDesc = Normalize(ctx.Description);
        return normalizedDesc.Contains(normalizedPattern, StringComparison.OrdinalIgnoreCase);
    }
    
    private static string Normalize(string s) => 
        NonAlphanumeric.Replace(s.ToUpperInvariant(), "");
}

public record ContainsMatch(string Pattern) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) =>
        ctx.Description.Contains(Pattern, StringComparison.OrdinalIgnoreCase);
}

public record AnyOfMatch(IReadOnlyList<string> Patterns) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) =>
        Patterns.Any(p => ctx.NormalizedDescription.Contains(
            p.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase));
}

public record RegexMatch(Regex Pattern) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) =>
        Pattern.IsMatch(ctx.Description);
}

public record AmountCondition(decimal Threshold, ComparisonOp Op) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) => Op switch
    {
        ComparisonOp.GreaterThan => Math.Abs(ctx.Amount) > Threshold,
        ComparisonOp.LessThan => Math.Abs(ctx.Amount) < Threshold,
        ComparisonOp.GreaterOrEqual => Math.Abs(ctx.Amount) >= Threshold,
        ComparisonOp.LessOrEqual => Math.Abs(ctx.Amount) <= Threshold,
        _ => false
    };
}

public record AndExpression(IMatchExpression Left, IMatchExpression Right) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) => 
        Left.Matches(ctx) && Right.Matches(ctx);
}

public record OrExpression(IMatchExpression Left, IMatchExpression Right) : IMatchExpression
{
    public bool Matches(TransactionContext ctx) => 
        Left.Matches(ctx) || Right.Matches(ctx);
}
```

### Rule Application
```csharp
public sealed class RuleApplicationService
{
    public async Task<RuleApplicationResult> ApplyRulesAsync(
        Guid profileId,
        IEnumerable<Guid>? transactionIds = null,
        CancellationToken ct = default)
    {
        // 1. Load rules for profile
        var rulesText = await LoadRulesText(profileId, ct);
        var parsedRules = _parser.Parse(rulesText);
        
        // 2. Load transactions
        var transactions = await LoadTransactions(profileId, transactionIds, ct);
        
        // 3. Apply rules in priority order
        var matched = 0;
        foreach (var transaction in transactions)
        {
            if (transaction.IsManualOverride) continue;
            
            var ctx = new TransactionContext(
                transaction.Description,
                transaction.NormalizedDescription,
                transaction.Amount
            );
            
            var matchingRule = parsedRules
                .OrderBy(r => r.Priority)
                .FirstOrDefault(r => r.MatchExpression.Matches(ctx));
            
            if (matchingRule is not null)
            {
                transaction.CategoryId = matchingRule.CategoryId;
                transaction.MatchedRuleId = matchingRule.Id;
                transaction.Tags = matchingRule.Tags.ToList();
                matched++;
            }
        }
        
        // 4. Save changes
        await _db.SaveChangesAsync(ct);
        
        return new RuleApplicationResult(matched, transactions.Count - matched);
    }
}
```

## Testing Rules

Use the `/rules/test` endpoint to test a rule against sample transactions:

```json
POST /api/profiles/{id}/rules/test
{
  "rule": "[test]\nmatch = 'normalized(\"NETFLIX\")'\ncategory = \"Subscriptions\"",
  "sampleTransactions": [
    { "description": "NETFLIX.COM", "amount": -9.99 },
    { "description": "NET FLIX MONTHLY", "amount": -15.99 },
    { "description": "AMAZON PRIME", "amount": -7.99 }
  ]
}
```

Response:
```json
{
  "matches": [
    { "description": "NETFLIX.COM", "matched": true },
    { "description": "NET FLIX MONTHLY", "matched": true },
    { "description": "AMAZON PRIME", "matched": false }
  ]
}
```

Now help with: $ARGUMENTS
