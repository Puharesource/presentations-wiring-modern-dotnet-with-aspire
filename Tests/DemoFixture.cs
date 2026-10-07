using TUnit.Aspire;
using System.Net.Http.Json;
using Backend.Entities;

namespace Tests;

// One fresh stack per test run. The fixture owns startup, readiness, and cleanup.
public class DemoFixture : AspireFixture<Projects.AppHost>
{
    protected override string[] Args => ["--UseVolumes=false"];
    // The npm installer finishes successfully; only long-running app resources need health checks.
    protected override ResourceWaitBehavior WaitBehavior => ResourceWaitBehavior.Named;
    protected override IEnumerable<string> ResourcesToWaitFor() => ["api", "frontend", "appdb", "cache"];

    protected override TimeSpan ResourceTimeout => TimeSpan.FromMinutes(3);
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        using var api = CreateHttpClient("api");
        var users = await api.GetFromJsonAsync<User[]>("/users");
        if (users is not { Length: 0 })
            throw new InvalidOperationException("Each test run must start with an empty database.");
    }
}
