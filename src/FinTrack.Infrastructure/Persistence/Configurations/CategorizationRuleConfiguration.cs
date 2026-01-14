using FinTrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class CategorizationRuleConfiguration : IEntityTypeConfiguration<CategorizationRule>
{
    public void Configure(EntityTypeBuilder<CategorizationRule> builder)
    {
        builder.ToTable("categorization_rules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.RuleToml)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(r => r.ProfileId);
        builder.HasIndex(r => new { r.ProfileId, r.Priority });

        builder.HasOne(r => r.Profile)
            .WithMany(p => p.CategorizationRules)
            .HasForeignKey(r => r.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
