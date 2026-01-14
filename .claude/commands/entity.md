# Create Entity

Create a new Entity Framework Core entity with configuration for the FinTrack application.

## Arguments
- `$ARGUMENTS` - The entity name and basic properties (e.g., "ImportBatch with ProfileId, FileName, Status, CreatedAt")

## Instructions

When the user runs `/entity <EntityName> with <properties>`, create:

1. **Entity class** in `src/FinTrack.Core/Domain/Entities/`
   - Use records for immutable entities or classes for mutable ones
   - Include navigation properties
   - Use value objects where appropriate

2. **EF Configuration** in `src/FinTrack.Infrastructure/Persistence/Configurations/`
   - Table name (snake_case, plural)
   - Column mappings
   - Indexes
   - Relationships

3. **DbContext update** - Add DbSet to FinTrackDbContext

4. **Migration command** - Provide the command to create the migration

## Code Templates

### Entity
```csharp
// src/FinTrack.Core/Domain/Entities/ImportBatch.cs
namespace FinTrack.Core.Domain.Entities;

public sealed class ImportBatch
{
    public Guid Id { get; init; }
    public Guid ProfileId { get; init; }
    public Guid AccountId { get; init; }
    public required string FileName { get; init; }
    public required string OriginalFileName { get; init; }
    public ImportStatus Status { get; set; }
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int ErrorCount { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public Profile Profile { get; init; } = null!;
    public Account Account { get; init; } = null!;
    private readonly List<Transaction> _transactions = [];
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
}

public enum ImportStatus
{
    Pending,
    Analyzing,
    AwaitingConfirmation,
    Processing,
    Completed,
    Failed
}
```

### EF Configuration
```csharp
// src/FinTrack.Infrastructure/Persistence/Configurations/ImportBatchConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTrack.Core.Domain.Entities;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("import_batches");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasDefaultValueSql("uuidv7()");

        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ErrorDetails)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        // Indexes
        builder.HasIndex(x => x.ProfileId);
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);

        // Relationships
        builder.HasOne(x => x.Profile)
            .WithMany()
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Transactions)
            .WithOne()
            .HasForeignKey(t => t.ImportBatchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

### DbContext Update
```csharp
// Add to FinTrackDbContext.cs
public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
```

### PostgreSQL-Specific Features

Use these PostgreSQL features where appropriate:

```csharp
// Array columns (for tags)
builder.Property(x => x.Tags)
    .HasColumnType("text[]");

// JSONB for flexible data
builder.Property(x => x.Metadata)
    .HasColumnType("jsonb");

// GIN index for array containment
builder.HasIndex(x => x.Tags)
    .HasMethod("gin");

// Trigram index for fuzzy search
builder.HasIndex(x => x.NormalizedDescription)
    .HasMethod("gin")
    .HasOperators("gin_trgm_ops");

// Check constraint
builder.HasCheckConstraint(
    "ck_import_batch_counts",
    "imported_count + duplicate_count + error_count <= total_rows");
```

## After Creating

Run the following command to create the migration:

```bash
dotnet ef migrations add Add{EntityName} -p src/FinTrack.Infrastructure -s src/FinTrack.Host
```

Now create the entity for: $ARGUMENTS
