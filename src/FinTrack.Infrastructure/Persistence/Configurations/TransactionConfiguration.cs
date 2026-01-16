using FinTrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Date)
            .IsRequired();

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(500);

        // Computed column for normalized description (used for merchant grouping)
        builder.Property<string>("NormalizedDescription")
            .HasComputedColumnSql("UPPER(TRIM(description))", stored: true)
            .HasMaxLength(500);

        builder.Property(t => t.Notes)
            .HasColumnType("text");

        builder.Property(t => t.Tags)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder.Property(t => t.RawData)
            .HasColumnType("jsonb");

        builder.Property(t => t.DuplicateHash)
            .HasMaxLength(64);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => t.Date).IsDescending();
        builder.HasIndex(t => new { t.AccountId, t.Date }).IsDescending(false, true);
        builder.HasIndex(t => t.DuplicateHash);
        builder.HasIndex("NormalizedDescription");

        builder.HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
