var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("fintrack-postgres-data")
    .WithPgAdmin();

var fintrackDb = postgres.AddDatabase("fintrack");

builder.AddProject("fintrack-api", "../FinTrack.Host/FinTrack.Host.csproj")
    .WithReference(fintrackDb)
    .WaitFor(fintrackDb)
    .WithExternalHttpEndpoints();

builder.Build().Run();
