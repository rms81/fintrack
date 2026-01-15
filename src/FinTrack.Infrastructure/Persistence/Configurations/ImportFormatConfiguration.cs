using FinTrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class ImportFormatConfiguration : IEntityTypeConfiguration<ImportFormat>
{
    public void Configure(EntityTypeBuilder<ImportFormat> builder)
    {
        builder.ToTable("import_formats");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.BankName)
            .HasMaxLength(100);

        builder.Property(f => f.Mapping)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(f => f.ProfileId);
        builder.HasIndex(f => new { f.ProfileId, f.Name }).IsUnique();

        builder.HasOne(f => f.Profile)
            .WithMany(p => p.ImportFormats)
            .HasForeignKey(f => f.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
