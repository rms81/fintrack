using FinTrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Icon)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("folder");

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#6B7280");

        builder.Property(c => c.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(c => c.ProfileId);
        builder.HasIndex(c => c.ParentId);
        // Note: Name uniqueness is enforced at application level since subcategories
        // with the same name can exist under different parents

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Profile)
            .WithMany(c => c.Categories)
            .HasForeignKey(c => c.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
