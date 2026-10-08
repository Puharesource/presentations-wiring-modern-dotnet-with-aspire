using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql");

// Development keeps its data; tests pass --UseVolumes=false for a fresh environment.
if (builder.Configuration.GetValue("UseVolumes", true))
    sql.WithDataVolume("modernize-demo-sql-migrations-data");

var database = sql.AddDatabase("appdb");
var cache = builder.AddRedis("cache");

var api = builder.AddProject<Projects.Backend>("api")
    .WithReference(database).WaitFor(database)
    .WithReference(cache).WaitFor(cache)
    .WithHttpHealthCheck("/health")
    .WithUrl("/openapi/v1.json", "OpenAPI");

api.AddEFMigrations("api-migrations", "Backend.DbContexts.DemoDbContext")
    .WaitFor(database);

builder.AddViteApp("frontend", "../frontend")
    .WithReference(api)
    .WithEnvironment("BACKEND_ENDPOINT", api.GetEndpoint("http"))
    .WaitFor(api);

builder.Build().Run();
