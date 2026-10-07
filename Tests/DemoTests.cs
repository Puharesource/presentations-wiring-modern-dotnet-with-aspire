using System.Net.Http.Json;
using Backend.Entities;

namespace Tests;

[NotInParallel]
public class DemoTests
{
    [ClassDataSource<DemoFixture>(Shared = SharedType.PerTestSession)]
    public required DemoFixture Fixture { get; init; }

    [Test]
    public async Task RedisCachesReadsAndCreatingAUserInvalidatesTheCache(CancellationToken ct)
    {
        using var api = Fixture.CreateHttpClient("api");
        var user = new User(Guid.NewGuid(), "", $"API test {Guid.NewGuid():N}", "api@example.test");
        var created = await api.PostAsJsonAsync("/users", user, ct);
        await Assert.That(created.IsSuccessStatusCode).IsTrue();

        var first = await api.GetAsync("/users", ct);
        var second = await api.GetAsync("/users", ct);
        first.EnsureSuccessStatusCode();
        second.EnsureSuccessStatusCode();
        var firstRead = first.Headers.GetValues("X-Database-Read").Single();
        await Assert.That(second.Headers.GetValues("X-Database-Read").Single()).IsEqualTo(firstRead);

        var nextUser = user with { Id = Guid.NewGuid(), Name = $"Next {Guid.NewGuid():N}" };
        (await api.PostAsJsonAsync("/users", nextUser, ct)).EnsureSuccessStatusCode();
        var refreshed = await api.GetAsync("/users", ct);
        refreshed.EnsureSuccessStatusCode();
        await Assert.That(refreshed.Headers.GetValues("X-Database-Read").Single()).IsNotEqualTo(firstRead);
        var users = await refreshed.Content.ReadFromJsonAsync<User[]>(ct) ?? [];
        await Assert.That(users.Any(found => found.Id == nextUser.Id)).IsTrue();
    }
    [Test]
    public async Task APersonCanBeAddedFromReactAndSurvivesReload(CancellationToken ct)
    {
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        using var frontend = Fixture.CreateHttpClient("frontend");
        await page.GotoAsync(frontend.BaseAddress!.ToString());
        var name = $"Browser test {Guid.NewGuid():N}";
        await page.GetByLabel("Name", new() { Exact = true }).FillAsync(name);
        await page.GetByLabel("Email", new() { Exact = true }).FillAsync("browser@example.test");
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Add person" }).ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(page.GetByRole(Microsoft.Playwright.AriaRole.Heading, new() { Name = name })).ToBeVisibleAsync();
        await page.ReloadAsync();
        await Microsoft.Playwright.Assertions.Expect(page.GetByRole(Microsoft.Playwright.AriaRole.Heading, new() { Name = name })).ToBeVisibleAsync();
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Refresh", Exact = true }).ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(page.GetByRole(Microsoft.Playwright.AriaRole.Status)).ToHaveTextAsync("Redis reused the response");
    }
}

