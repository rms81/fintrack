using FinTrack.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence;

public class FinTrackDbContext(DbContextOptions<FinTrackDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<CategorizationRule> CategorizationRules => Set<CategorizationRule>();
    public DbSet<ImportSession> ImportSessions => Set<ImportSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity tables to use snake_case naming
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("asp_net_users");
        });

        modelBuilder.Entity<IdentityRole<Guid>>(b =>
        {
            b.ToTable("asp_net_roles");
        });

        modelBuilder.Entity<IdentityUserRole<Guid>>(b =>
        {
            b.ToTable("asp_net_user_roles");
        });

        modelBuilder.Entity<IdentityUserClaim<Guid>>(b =>
        {
            b.ToTable("asp_net_user_claims");
        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>(b =>
        {
            b.ToTable("asp_net_user_logins");
        });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(b =>
        {
            b.ToTable("asp_net_role_claims");
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(b =>
        {
            b.ToTable("asp_net_user_tokens");
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinTrackDbContext).Assembly);

        // Enable pg_trgm extension for text search
        modelBuilder.HasPostgresExtension("pg_trgm");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
