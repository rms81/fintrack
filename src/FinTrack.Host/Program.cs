using FinTrack.Host.Auth;
using FinTrack.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFinTrackAuthentication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "FinTrack API")
    .WithName("Root");

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
    .WithName("Health");

app.Run();

// Make the implicit Program class public for WebApplicationFactory
public partial class Program { }
