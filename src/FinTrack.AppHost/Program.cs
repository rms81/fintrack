var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("fintrack-postgres-data")
    .WithPgAdmin();

var fintrackDb = postgres.AddDatabase("fintrack");

// Add FinTrack Host API with reference to database
// Map the Aspire connection string name to what the app expects
var api = builder.AddProject<Projects.FinTrack_Host>("fintrack-api")
    .WithReference(fintrackDb, "DefaultConnection")
    .WaitFor(fintrackDb);

builder.Build().Run();
