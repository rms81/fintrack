using FinTrack.Core.Features.Example;
using FinTrack.Host.Auth;
using FinTrack.Host.Endpoints;
using FinTrack.Host.Exceptions;
using FinTrack.Host.Middleware;
using FinTrack.Host.Security;
using FinTrack.Infrastructure;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, resilience)
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "FinTrack API",
            Version = "v1",
            Description = "Self-hosted expense tracking API for individuals and sole proprietors. " +
                          "Manage profiles, accounts, transactions, categories, and import bank statements.",
            Contact = new()
            {
                Name = "FinTrack Support",
                Email = "support@fintrack.local"
            },
            License = new()
            {
                Name = "MIT"
            }
        };
        return Task.CompletedTask;
    });
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFinTrackAuthentication(builder.Configuration);

// Configure Wolverine
builder.Host.UseWolverine(opts =>
{
    // Discover handlers from Core assembly
    opts.Discovery.IncludeAssembly(typeof(PingQuery).Assembly);

    // Configure local queue for commands
    opts.LocalQueue("default")
        .UseDurableInbox();

    // Policies
    opts.Policies.AutoApplyTransactions();
});

// Add Wolverine HTTP integration
builder.Services.AddWolverineHttp();

var app = builder.Build();

// Apply pending migrations in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinTrackDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline
app.UseCorrelationId();
app.UseRequestLogging();
app.UseAppExceptionHandler();
app.UseSecurityHeaders();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("FinTrack API")
           .WithTheme(ScalarTheme.BluePlanet)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseAuthentication();
app.UseAuthorization();

// Map Wolverine HTTP endpoints
app.MapWolverineEndpoints();

// Basic endpoints
app.MapGet("/", () => "FinTrack API")
    .WithName("Root")
    .WithTags("System")
    .WithSummary("API Root")
    .WithDescription("Returns the API name. Use this to verify the API is running.")
    .ExcludeFromDescription();

// Map Aspire default endpoints (health checks)
app.MapDefaultEndpoints();

// Map auth endpoints
app.MapAuthEndpoints();

app.Run();

// Make the implicit Program class public for WebApplicationFactory
public partial class Program { }
