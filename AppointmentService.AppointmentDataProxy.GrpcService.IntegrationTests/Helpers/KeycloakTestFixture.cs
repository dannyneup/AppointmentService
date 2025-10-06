using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;

public class KeycloakTestFixture : IAsyncLifetime
{
    public string Realm { get; } = "appointment-data";
    public string Audience => "appointment-data-proxy-grpc-service";

    private IContainer _container = null!;
    private bool _initialized;

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;
        _container = new ContainerBuilder()
            .WithImage("quay.io/keycloak/keycloak:26.4")
            .WithPortBinding(8080, assignRandomHostPort: true)
            .WithEnvironment("KEYCLOAK_ADMIN", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithBindMount(Path.GetFullPath("realm-export.json"), "/opt/keycloak/data/import/realm-export.json", AccessMode.ReadOnly)
            .WithCommand("start-dev", "--import-realm")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(strategy => strategy.ForPort(8080).ForPath("/")))
            .Build();

        await _container.StartAsync();
        _initialized = true;
    }

    public async Task DisposeAsync() => await _container.DisposeAsync().AsTask();

    public async Task<string> GetBaseUrlAsync()
    {
        await InitializeAsync();
        return $"http://localhost:{_container.GetMappedPublicPort()}";
    }

    public async Task<string> GetAuthorityAsync()
    {
        await InitializeAsync();
        var baseUrl = await GetBaseUrlAsync();
        return $"{baseUrl}/realms/{Realm}";
    }

    public async Task<string> GetClientCredentialsTokenAsync(string clientId,
        string clientSecret,
        CancellationToken cancellationToken,
        IEnumerable<string>? scopes = null)
    {
        using var client = new HttpClient();
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };
        if (scopes is not null)
            form["scope"] = string.Join(' ', scopes);

        var authority = await GetAuthorityAsync();
        var response = await client.PostAsync($"{authority}/protocol/openid-connect/token", new FormUrlEncodedContent(form), cancellationToken);
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }
}