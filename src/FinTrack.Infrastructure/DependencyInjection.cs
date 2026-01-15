using FinTrack.Core.Services;
using FinTrack.Infrastructure.Options;
using FinTrack.Infrastructure.Persistence;
using FinTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FinTrackDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(FinTrackDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(3);
                });

            options.UseSnakeCaseNamingConvention();

            // Suppress warning about pending model changes (occurs with manually created migrations)
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        // Register Rules Engine
        services.AddScoped<IRulesEngine, RulesEngine>();

        // Register Import Service
        services.AddScoped<IImportService, CsvImportService>();

        // Register Category Seeder
        services.AddSingleton<ICategorySeeder, CategorySeeder>();

        // Register LLM Service (OpenRouter or Stub)
        var llmSection = configuration.GetSection(LlmOptions.Section);
        if (llmSection.Exists() && !string.IsNullOrEmpty(llmSection["ApiKey"]))
        {
            services.Configure<LlmOptions>(options => llmSection.Bind(options));
            services.AddHttpClient<ILlmService, OpenRouterService>(client =>
            {
                var options = llmSection.Get<LlmOptions>()!;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
                client.DefaultRequestHeaders.Add("HTTP-Referer", "https://fintrack.local");
            });
        }
        else
        {
            // Register stub when no LLM API key is configured
            services.AddSingleton<ILlmService, StubLlmService>();
        }

        return services;
    }
}
