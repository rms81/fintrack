using FinTrack.Infrastructure.Persistence;
using FinTrack.Tests.Integration.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace FinTrack.Tests.Integration;

public class FinTrackWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18")
        .WithDatabase("fintrack_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FinTrackDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Add test database
            services.AddDbContext<FinTrackDbContext>(options =>
            {
                options.UseNpgsql(_postgres.GetConnectionString());
                options.UseSnakeCaseNamingConvention();
            });

            // Replace authentication with test handler
            services.AddTestAuthentication();

            // Ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FinTrackDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }
}
