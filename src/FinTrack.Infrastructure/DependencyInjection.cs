using FinTrack.Core.Services;
using FinTrack.Infrastructure.Options;
using FinTrack.Infrastructure.Persistence;
using FinTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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
        });

        // Register Rules Engine
        services.AddScoped<IRulesEngine, RulesEngine>();

        // Register Import Service
        services.AddScoped<IImportService, CsvImportService>();

        // Register LLM Service (OpenRouter)
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

        return services;
    }
}
