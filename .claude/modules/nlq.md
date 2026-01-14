# Natural Language Query Module

> **Phase:** 4 (NLQ & Export)
> **Status:** Planned

## Overview
Allow users to query their financial data using natural language, translated to SQL via LLM.

## Examples

| User Query | Generated SQL |
|------------|---------------|
| "How much did I spend on food last month?" | `SELECT SUM(amount) FROM transactions WHERE category_id = '...' AND date >= '2024-01-01'` |
| "Show my largest expenses this year" | `SELECT * FROM transactions WHERE amount < 0 ORDER BY amount ASC LIMIT 10` |
| "What's my average monthly spending?" | `SELECT AVG(monthly_total) FROM (SELECT SUM(amount) as monthly_total FROM transactions GROUP BY date_trunc('month', date))` |

## Flow

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐     ┌──────────────┐
│ User Query  │ ──▶ │ LLM Translate│ ──▶ │ Validate SQL│ ──▶ │ Execute Query│
└─────────────┘     └──────────────┘     └─────────────┘     └──────────────┘
                          │                     │
                          ▼                     ▼
                    ┌──────────────┐     ┌──────────────┐
                    │ Schema Ctx   │     │ Sanitize     │
                    └──────────────┘     └──────────────┘
```

## Security

<!-- TODO: Phase 4 - Critical security measures -->

1. **Read-only queries only** - Reject INSERT, UPDATE, DELETE
2. **Schema context** - Only expose allowed tables/columns
3. **Profile scoping** - Always filter by user's profile
4. **Query validation** - Parse and validate before execution
5. **Timeout** - Kill long-running queries
6. **Result limits** - Max 1000 rows returned

## LLM Prompt

See: `integrations/openrouter.md`

Schema context includes:
- Available tables and columns
- Profile-specific account IDs
- Category names for reference

## Endpoints

<!-- TODO: Phase 4 -->

### POST /api/profiles/{profileId}/query
Execute natural language query.

**Request:**
```json
{
  "query": "How much did I spend on transportation last month?"
}
```

**Response:**
```json
{
  "sql": "SELECT SUM(amount) FROM transactions WHERE...",
  "results": [{ "sum": -245.50 }],
  "explanation": "Total transportation expenses for January 2024"
}
```

## Query History

<!-- TODO: Phase 4 -->

Store queries for:
- Quick re-run
- Suggest similar queries
- Analytics on common patterns

## React Components

<!-- TODO: Phase 4 -->

- Natural language input with suggestions
- Query result table/chart
- SQL preview (collapsible)
- Query history sidebar
