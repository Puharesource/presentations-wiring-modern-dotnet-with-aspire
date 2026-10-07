using TUnit.Aspire;

namespace Tests;

// One fresh stack per test run. The fixture owns startup, readiness, and cleanup.
public class DemoFixture : AspireFixture<Projects.AppHost>
{
    protected override string[] Args => ["--UseVolumes=false"];
    // The npm installer finishes successfully; only long-running app resources need health checks.
    protected override ResourceWaitBehavior WaitBehavior => ResourceWaitBehavior.Named;
    protected override IEnumerable<string> ResourcesToWaitFor() => ["api", "frontend", "appdb", "cache"];

    protected override TimeSpan ResourceTimeout => TimeSpan.FromMinutes(3);
}
