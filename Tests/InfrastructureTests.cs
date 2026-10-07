using Microsoft.Data.SqlClient;
using StackExchange.Redis;

namespace Tests;

[NotInParallel]
public class InfrastructureTests
{
    [ClassDataSource<DemoFixture>(Shared = SharedType.PerTestSession)]
    public required DemoFixture Fixture { get; init; }

    [Test]
    public async Task ConnectionStringsComeFromTheRunningAppHost(CancellationToken ct)
    {
        // No fixed ports, credentials, or manual environment variable parsing.
        await using var sql = new SqlConnection(await Fixture.GetConnectionStringAsync("appdb", ct));
        await sql.OpenAsync(ct);
        await using var query = new SqlCommand("SELECT COUNT(*) FROM Users", sql);
        await Assert.That(await query.ExecuteScalarAsync(ct)).IsNotNull();

        using var redis = await ConnectionMultiplexer.ConnectAsync(
            (await Fixture.GetConnectionStringAsync("cache", ct))!);
        await Assert.That(await redis.GetDatabase().PingAsync()).IsGreaterThan(TimeSpan.Zero);
    }
}
