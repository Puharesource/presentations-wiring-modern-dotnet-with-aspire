using Microsoft.Extensions.Configuration;

namespace Tests;

// Compose must already be running. The before branch does not own its environment.
public class DemoFixture
{
    private readonly IConfiguration config = new ConfigurationBuilder().AddEnvironmentVariables().Build();

    public HttpClient CreateHttpClient(string resource) => new()
    {
        BaseAddress = new Uri(config[$"{resource.ToUpperInvariant()}_URL"] ??
            (resource == "api" ? "http://localhost:5244" : "http://localhost:5173"))
    };

    public Task<string?> GetConnectionStringAsync(string resource) =>
        Task.FromResult(config.GetConnectionString(resource));
}
