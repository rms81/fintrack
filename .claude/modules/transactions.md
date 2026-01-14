# Transactions Module

> **Phase:** 2 (Import & Rules) and 3 (Dashboard)
> **Status:** Planned

## Overview
Transactions are the core data entity representing financial movements (expenses and income).

## Domain Model

```csharp
public class Transaction : BaseEntity
{
    public Guid AccountId { get; init; }
    public Guid? CategoryId { get; set; }
    public required DateOnly Date { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public string? Notes { get; set; }
    public string[] Tags { get; set; } = [];
    public JsonDocument? RawData { get; set; }
    
    // Navigation
    public Account Account { get; init; } = null!;
    public Category? Category { get; set; }
}
```

## Database

### Table: transactions
```sql
CREATE TABLE transactions (
    id uuid PRIMARY KEY DEFAULT uuidv7(),
    account_id uuid NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
    category_id uuid REFERENCES categories(id) ON DELETE SET NULL,
    date date NOT NULL,
    amount decimal(18,2) NOT NULL,
    description varchar(500) NOT NULL,
    notes text,
    tags text[] NOT NULL DEFAULT '{}',
    raw_data jsonb,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_transactions_account_id ON transactions(account_id);
CREATE INDEX ix_transactions_category_id ON transactions(category_id);
CREATE INDEX ix_transactions_date ON transactions(date DESC);
CREATE INDEX ix_transactions_account_date ON transactions(account_id, date DESC);
CREATE INDEX ix_transactions_description_fts 
    ON transactions USING gin(to_tsvector('english', description));
```

## Endpoints

<!-- TODO: Phase 2/3 - Define endpoints -->

### GET /api/profiles/{profileId}/transactions
List transactions with filtering and pagination.

### GET /api/transactions/{id}
Get single transaction.

### PUT /api/transactions/{id}
Update transaction (category, notes, tags).

### DELETE /api/transactions/{id}
Delete transaction.

## Filtering

<!-- TODO: Phase 3 - Implement filtering -->

Query parameters:
- `accountId` - Filter by account
- `categoryId` - Filter by category
- `fromDate` / `toDate` - Date range
- `minAmount` / `maxAmount` - Amount range
- `search` - Full-text search on description
- `tags` - Filter by tags (comma-separated)
- `uncategorized=true` - Only uncategorized

## DTOs

<!-- TODO: Define DTOs -->

## Handlers

<!-- TODO: Define Wolverine handlers -->

## React Components

<!-- TODO: Phase 3 - Transaction list, filters, detail view -->
