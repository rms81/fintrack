# Database Architecture

## Overview
FinTrack uses PostgreSQL 18 with Entity Framework Core 10 (Code-First approach).

## Connection

### Development
```
Host=localhost;Database=fintrack_dev;Username=fintrack;Password=secret
```

### Production
```
Host=postgres;Database=fintrack;Username=fintrack;Password=${POSTGRES_PASSWORD}
```

## Schema Conventions

### Naming
| Element | Convention | Example |
|---------|------------|---------|
| Tables | snake_case, plural | `transactions`, `profiles` |
| Columns | snake_case | `created_at`, `profile_id` |
| Primary Keys | `id` | `id uuid PRIMARY KEY` |
| Foreign Keys | `{table}_id` | `profile_id`, `category_id` |
| Indexes | `ix_{table}_{columns}` | `ix_transactions_profile_id` |

### Primary Keys
Using PostgreSQL native UUIDv7 for time-ordered unique IDs:

```sql
-- Enable extension (if needed)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- UUIDv7 function
CREATE OR REPLACE FUNCTION uuidv7() RETURNS uuid AS $$
  SELECT encode(
    set_bit(
      set_bit(
        overlay(
          uuid_send(gen_random_uuid())
          placing substring(int8send((extract(epoch FROM clock_timestamp()) * 1000)::bigint) from 3)
          from 1 for 6
        ),
        52, 1
      ),
      53, 1
    ),
    'hex'
  )::uuid;
$$ LANGUAGE sql VOLATILE;
```

### Timestamps
All tables include:
```sql
created_at timestamptz NOT NULL DEFAULT now(),
updated_at timestamptz NOT NULL DEFAULT now()
```

## Entity Configurations

### Base Entity
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
```

### EF Core Configuration Pattern
```csharp
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasDefaultValueSql("uuidv7()");
            
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");
            
        // Relationships
        builder.HasOne(x => x.Profile)
            .WithMany(p => p.Transactions)
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

## Tables

### profiles
Primary entity for user's financial profiles (Personal/Business).

| Column | Type | Notes |
|--------|------|-------|
| id | uuid | PK, UUIDv7 |
| user_id | uuid | FK to identity user |
| name | varchar(100) | Profile display name |
| type | varchar(20) | 'personal' or 'business' |
| created_at | timestamptz | Auto-set |
| updated_at | timestamptz | Auto-updated |

### accounts
Bank accounts linked to profiles.

| Column | Type | Notes |
|--------|------|-------|
| id | uuid | PK, UUIDv7 |
| profile_id | uuid | FK to profiles |
| name | varchar(100) | Account display name |
| institution | varchar(100) | Bank name |
| account_number_masked | varchar(20) | Last 4 digits only |
| currency | char(3) | ISO 4217 (EUR, USD) |
| created_at | timestamptz | Auto-set |
| updated_at | timestamptz | Auto-updated |

### categories
Transaction categories for organization.

| Column | Type | Notes |
|--------|------|-------|
| id | uuid | PK, UUIDv7 |
| profile_id | uuid | FK to profiles |
| name | varchar(100) | Category name |
| icon | varchar(50) | Icon identifier |
| color | char(7) | Hex color (#FF5733) |
| parent_id | uuid | FK self-reference (nullable) |
| created_at | timestamptz | Auto-set |
| updated_at | timestamptz | Auto-updated |

### transactions
Financial transactions (expenses/income).

| Column | Type | Notes |
|--------|------|-------|
| id | uuid | PK, UUIDv7 |
| account_id | uuid | FK to accounts |
| category_id | uuid | FK to categories (nullable) |
| date | date | Transaction date |
| amount | decimal(18,2) | Signed amount |
| description | varchar(500) | Original description |
| notes | text | User notes (nullable) |
| tags | text[] | PostgreSQL array |
| raw_data | jsonb | Original CSV row |
| created_at | timestamptz | Auto-set |
| updated_at | timestamptz | Auto-updated |

### categorization_rules
TOML-based rules for automatic categorization.

| Column | Type | Notes |
|--------|------|-------|
| id | uuid | PK, UUIDv7 |
| profile_id | uuid | FK to profiles |
| name | varchar(100) | Rule name |
| priority | int | Execution order |
| rule_toml | text | TOML rule definition |
| is_active | boolean | Enabled/disabled |
| created_at | timestamptz | Auto-set |
| updated_at | timestamptz | Auto-updated |

### import_sessions
Track CSV import history.

| Column | Type | Notes |
|--------|------|-------|
| id | uuid | PK, UUIDv7 |
| account_id | uuid | FK to accounts |
| filename | varchar(255) | Original filename |
| row_count | int | Total rows imported |
| status | varchar(20) | pending/completed/failed |
| error_message | text | If failed |
| format_config | jsonb | Detected CSV format |
| created_at | timestamptz | Auto-set |
| updated_at | timestamptz | Auto-updated |

## Indexes

```sql
-- Performance indexes
CREATE INDEX ix_transactions_account_id ON transactions(account_id);
CREATE INDEX ix_transactions_date ON transactions(date DESC);
CREATE INDEX ix_transactions_category_id ON transactions(category_id);
CREATE INDEX ix_accounts_profile_id ON accounts(profile_id);
CREATE INDEX ix_categories_profile_id ON categories(profile_id);

-- Full-text search on descriptions
CREATE INDEX ix_transactions_description_gin ON transactions 
  USING gin(to_tsvector('english', description));
```

## Migrations

### Create New Migration
```bash
dotnet ef migrations add <Name> -p src/FinTrack.Infrastructure -s src/FinTrack.Host
```

### Apply Migrations
```bash
dotnet ef database update -p src/FinTrack.Infrastructure -s src/FinTrack.Host
```

### Rollback
```bash
dotnet ef database update <PreviousMigrationName> -p src/FinTrack.Infrastructure -s src/FinTrack.Host
```

### Generate SQL Script
```bash
dotnet ef migrations script -p src/FinTrack.Infrastructure -s src/FinTrack.Host -o migration.sql
```
