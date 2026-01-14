# PostgreSQL Integration

## Overview
FinTrack uses PostgreSQL 18 with Entity Framework Core 10 for data persistence.

## Connection

### Npgsql Configuration
```csharp
builder.Services.AddDbContext<FinTrackDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql =>
        {
            npgsql.MigrationsAssembly("FinTrack.Infrastructure");
            npgsql.EnableRetryOnFailure(3);
            npgsql.CommandTimeout(30);
        });
});
```

### Connection String Format
```
Host=localhost;Database=fintrack;Username=fintrack;Password=secret;
Include Error Detail=true;  # Development only
Pooling=true;
Minimum Pool Size=5;
Maximum Pool Size=100;
```

## PostgreSQL-Specific Features

### UUIDv7 Primary Keys
Time-ordered UUIDs for better index performance:

```sql
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

EF Core configuration:
```csharp
builder.Property(x => x.Id)
    .HasDefaultValueSql("uuidv7()");
```

### Array Columns (text[])
For transaction tags:

```csharp
// Entity
public string[] Tags { get; set; } = [];

// Configuration
builder.Property(x => x.Tags)
    .HasColumnType("text[]");

// Query
var tagged = await db.Transactions
    .Where(t => t.Tags.Contains("groceries"))
    .ToListAsync();
```

### JSONB Columns
For flexible data like raw CSV rows:

```csharp
// Entity
public JsonDocument? RawData { get; set; }

// Configuration
builder.Property(x => x.RawData)
    .HasColumnType("jsonb");

// Query JSON properties
var filtered = await db.Transactions
    .Where(t => EF.Functions.JsonContains(
        t.RawData, 
        JsonDocument.Parse("{\"type\": \"transfer\"}")))
    .ToListAsync();
```

### Full-Text Search
For transaction descriptions:

```sql
-- Create index
CREATE INDEX ix_transactions_description_fts 
ON transactions USING gin(to_tsvector('english', description));
```

```csharp
// Query
var results = await db.Transactions
    .Where(t => EF.Functions.ToTsVector("english", t.Description)
        .Matches(EF.Functions.PlainToTsQuery("english", searchTerm)))
    .ToListAsync();
```

### Date Ranges
Using PostgreSQL's daterange:

```csharp
// For date-based queries
var monthly = await db.Transactions
    .Where(t => t.Date >= startDate && t.Date <= endDate)
    .ToListAsync();
```

## Performance Optimization

### Indexes
```sql
-- Foreign key indexes (always add these)
CREATE INDEX ix_transactions_account_id ON transactions(account_id);
CREATE INDEX ix_transactions_category_id ON transactions(category_id);
CREATE INDEX ix_accounts_profile_id ON accounts(profile_id);

-- Query pattern indexes
CREATE INDEX ix_transactions_date ON transactions(date DESC);
CREATE INDEX ix_transactions_account_date ON transactions(account_id, date DESC);

-- Partial indexes for active records
CREATE INDEX ix_accounts_active ON accounts(profile_id) WHERE is_active = true;
```

### Query Optimization

**Avoid N+1 with explicit includes:**
```csharp
var transactions = await db.Transactions
    .Include(t => t.Category)
    .Include(t => t.Account)
    .Where(t => t.Account.ProfileId == profileId)
    .ToListAsync();
```

**Use projections for list views:**
```csharp
var list = await db.Transactions
    .Where(t => t.Account.ProfileId == profileId)
    .Select(t => new TransactionListDto(
        t.Id,
        t.Date,
        t.Description,
        t.Amount,
        t.Category != null ? t.Category.Name : null
    ))
    .ToListAsync();
```

**Pagination:**
```csharp
var page = await db.Transactions
    .OrderByDescending(t => t.Date)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### Bulk Operations
For CSV imports:

```csharp
// Use ExecuteUpdate for bulk updates
await db.Transactions
    .Where(t => t.CategoryId == null && t.Description.Contains("UBER"))
    .ExecuteUpdateAsync(s => s.SetProperty(
        t => t.CategoryId, 
        transportCategoryId));

// Use AddRange for bulk inserts
db.Transactions.AddRange(importedTransactions);
await db.SaveChangesAsync();
```

## Migrations

### Create Migration
```bash
dotnet ef migrations add <n> \
  -p src/FinTrack.Infrastructure \
  -s src/FinTrack.Host
```

### Apply Migration
```bash
dotnet ef database update \
  -p src/FinTrack.Infrastructure \
  -s src/FinTrack.Host
```

### Generate Script (for production)
```bash
dotnet ef migrations script \
  -p src/FinTrack.Infrastructure \
  -s src/FinTrack.Host \
  --idempotent \
  -o migration.sql
```

### Rollback
```bash
dotnet ef database update <PreviousMigrationName> \
  -p src/FinTrack.Infrastructure \
  -s src/FinTrack.Host
```

## Naming Conventions

EF Core to PostgreSQL naming convention:

```csharp
// In DbContext OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply snake_case naming
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        // Table name
        entity.SetTableName(entity.GetTableName()?.ToSnakeCase());
        
        // Column names
        foreach (var property in entity.GetProperties())
        {
            property.SetColumnName(property.GetColumnName()?.ToSnakeCase());
        }
        
        // Key names
        foreach (var key in entity.GetKeys())
        {
            key.SetName(key.GetName()?.ToSnakeCase());
        }
        
        // Foreign key names
        foreach (var fk in entity.GetForeignKeys())
        {
            fk.SetConstraintName(fk.GetConstraintName()?.ToSnakeCase());
        }
        
        // Index names
        foreach (var index in entity.GetIndexes())
        {
            index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
        }
    }
}
```

Or use the `EFCore.NamingConventions` package:
```csharp
options.UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention();
```

## Health Check

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres");

app.MapHealthChecks("/health");
```

## Troubleshooting

### Connection Issues
```bash
# Test connection
psql -h localhost -U fintrack -d fintrack -c "SELECT 1"

# Check PostgreSQL logs
docker compose logs postgres
```

### Slow Queries
```sql
-- Enable query logging
ALTER SYSTEM SET log_min_duration_statement = 100;
SELECT pg_reload_conf();

-- Check for missing indexes
SELECT schemaname, tablename, indexname 
FROM pg_indexes 
WHERE tablename = 'transactions';

-- Analyze query plan
EXPLAIN ANALYZE SELECT * FROM transactions WHERE date > '2024-01-01';
```

### Lock Issues
```sql
-- View locks
SELECT * FROM pg_locks WHERE NOT granted;

-- Kill blocking session
SELECT pg_terminate_backend(<pid>);
```
