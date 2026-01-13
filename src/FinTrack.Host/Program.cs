using FinTrack.Core.Features.Example;
using FinTrack.Host.Auth;
using FinTrack.Infrastructure;
using Wolverine;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

// Map Wolverine HTTP endpoints
app.MapWolverineEndpoints();

// Basic endpoints
app.MapGet("/", () => "FinTrack API")
    .WithName("Root");

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
    .WithName("Health");

app.Run();

// Make the implicit Program class public for WebApplicationFactory
public partial class Program { }
