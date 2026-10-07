using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithImageTag("2022-latest")
    .WithContainerRuntimeArgs("--platform", "linux/amd64");

// Development keeps its data; tests pass --UseVolumes=false for a fresh environment.
if (builder.Configuration.GetValue("UseVolumes", true))
    sql.WithDataVolume("modernize-demo-sql-data");

var database = sql.AddDatabase("appdb");
var cache = builder.AddRedis("cache");

var api = builder.AddProject<Projects.Backend>("api", launchProfileName: "http")
    .WithReference(database).WaitFor(database)
    .WithReference(cache).WaitFor(cache)
    .WithHttpHealthCheck("/health");

builder.AddViteApp("frontend", "../frontend")
    .WithReference(api)
    .WithEnvironment("BACKEND_ENDPOINT", api.GetEndpoint("http"))
    .WaitFor(api);

builder.Build().Run();
