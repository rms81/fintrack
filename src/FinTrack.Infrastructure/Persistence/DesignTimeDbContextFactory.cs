using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinTrack.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinTrackDbContext>
{
    public FinTrackDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinTrackDbContext>();

        // Use a default connection string for design-time operations
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Database=fintrack;Username=fintrack;Password=fintrack_secret";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(FinTrackDbContext).Assembly.FullName);
        });

        optionsBuilder.UseSnakeCaseNamingConvention();

        return new FinTrackDbContext(optionsBuilder.Options);
    }
}
