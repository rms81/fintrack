using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class ImportSessionConfiguration : IEntityTypeConfiguration<ImportSession>
{
    public void Configure(EntityTypeBuilder<ImportSession> builder)
    {
        builder.ToTable("import_sessions");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.Filename)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(i => i.RowCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ImportStatus.Pending);

        builder.Property(i => i.ErrorMessage)
            .HasColumnType("text");

        builder.Property(i => i.FormatConfig)
            .HasColumnType("jsonb");

        builder.Property(i => i.CsvData)
            .HasColumnType("bytea");

        builder.Property(i => i.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(i => i.AccountId);
        builder.HasIndex(i => i.Status);

        builder.HasOne(i => i.Account)
            .WithMany(a => a.ImportSessions)
            .HasForeignKey(i => i.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
