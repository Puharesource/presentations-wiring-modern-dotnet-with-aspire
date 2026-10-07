using Backend.DbContexts;
using Backend.Entities;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSqlServerDbContext<DemoDbContext>("appdb");
builder.AddRedisOutputCache("cache");
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseOutputCache();
app.MapDefaultEndpoints();
app.MapOpenApi();

// This header changes only when SQL is queried; Redis replays it with the response.
app.MapGet("/users", async (DemoDbContext db, HttpContext context, CancellationToken ct) =>
{
    var users = await db.Users.AsNoTracking().OrderBy(user => user.Name).ToListAsync(ct);
    context.Response.Headers["X-Database-Read"] = Guid.NewGuid().ToString();
    return Results.Ok(users);
}).CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(1)).Tag("users"));

app.MapPost("/users", async (User user, DemoDbContext db, IOutputCacheStore cache, CancellationToken ct) =>
{
    if (user.Id == Guid.Empty || string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email))
        return Results.BadRequest("Id, name, and email are required.");

    db.Users.Add(user);
    await db.SaveChangesAsync(ct);
    await cache.EvictByTagAsync("users", ct);
    return Results.Created($"/users/{user.Id}", user);
});

// EnsureCreated keeps this disposable teaching app small; production apps use migrations.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await db.Database.EnsureCreatedAsync();
}

await app.RunAsync();
