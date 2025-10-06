using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;

public abstract class IntegrationTestBase : IClassFixture<GrpcServiceTestFixture<Program>>,
    IClassFixture<KeycloakTestFixture>,
    IAsyncLifetime
{
    internal readonly GrpcTestContext<Program> Context;
    private GrpcChannel? _channel;
    private string _authorizedBearerToken;

    protected readonly GrpcServiceTestFixture<Program> ServiceFixture;
    protected readonly KeycloakTestFixture KeycloakFixture;

    private Action<IWebHostBuilder>? _hostConfiguration;

    private const string TestClientId = "test-client";
    private const string TestClientSecret = "dummy-test-secret";

    protected IntegrationTestBase(GrpcServiceTestFixture<Program> serviceTestFixture,
        KeycloakTestFixture keycloakTestFixture,
        ITestOutputHelper outputHelper)
    {
        Context = serviceTestFixture.GetTestContext(outputHelper);
        ServiceFixture = serviceTestFixture;
        KeycloakFixture = keycloakTestFixture;
    }

    protected void AddAdditionalHostConfiguration(Action<IWebHostBuilder> additionalHostConfiguration)
    {
        ServiceFixture.ConfigureWebHost(host =>
        {
            _hostConfiguration?.Invoke(host);
            additionalHostConfiguration(host);
        });
    }

    protected Action<IWebHostBuilder> ReplaceOption<TOption>(TOption setting) where TOption : class
        => host
            => host.ConfigureServices(services
                => services.Replace(
                    new ServiceDescriptor(
                        typeof(IOptions<TOption>),
                        Options.Create(setting)
                    )
                )
            );

    protected GrpcChannel Channel => _channel ??= CreateChannel();

    private GrpcChannel CreateChannel()
    {
        var credentials = CallCredentials.FromInterceptor((_, metadata) =>
        {
            metadata.Add("authorization", $"Bearer {_authorizedBearerToken}");
            return Task.CompletedTask;
        });
        var channel = GrpcChannel.ForAddress("http://localhost",
            new GrpcChannelOptions
            {
                LoggerFactory = ServiceFixture.LoggerFactory,
                HttpHandler = ServiceFixture.Handler,
                Credentials = ChannelCredentials.Create(ChannelCredentials.Insecure, credentials),
                UnsafeUseInsecureChannelCallCredentials = true
            });
        return channel;
    }


    public async Task InitializeAsync()
    {
        _authorizedBearerToken =
            await KeycloakFixture.GetClientCredentialsTokenAsync(
                TestClientId,
                TestClientSecret,
                CancellationToken.None);

        await Task.WhenAll(
            ApplyInitScript(ServiceFixture.CentralDatabaseContainer.GetConnectionString(), "central.sql"),
            ApplyInitScript(ServiceFixture.CompanyDatabaseContainer.GetConnectionString(), "company.sql")
        );
        _hostConfiguration = await CreatHostConfigurationAsync();
        ServiceFixture.ConfigureWebHost(_hostConfiguration);
    }

    public async Task DisposeAsync()
    {
        await ResetDatabases();
        Context.Dispose();
        if (_channel != null)
        {
            await _channel.ShutdownAsync();
            _channel.Dispose();
            _channel = null;
        }
    }

    private Task ResetDatabases() =>
        Task.WhenAll(
            DropDatabaseSchema(ServiceFixture.CompanyDatabaseContainer.GetConnectionString()),
            DropDatabaseSchema(ServiceFixture.CentralDatabaseContainer.GetConnectionString())
        );

    private static async Task ApplyInitScript(string connectionString, string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"Initialization script not found: {scriptPath}");
        }

        var scriptContent = await File.ReadAllTextAsync(scriptPath);
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = scriptContent;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DropDatabaseSchema(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        const string dropTablesQuery = """

                                                   drop schema public cascade;
                                                   create schema public;
                                               
                                       """;

        await using var command = connection.CreateCommand();
        command.CommandText = dropTablesQuery;
        await command.ExecuteNonQueryAsync();
        await Task.CompletedTask;
    }

    private async Task<Action<IWebHostBuilder>> CreatHostConfigurationAsync()
    {
        var keycloakAuthority = await KeycloakFixture.GetAuthorityAsync();
        return host =>
        {
            host.ConfigureAppConfiguration(configure => configure.AddInMemoryCollection(
                [
                    new(
                        $"{AuthenticationSettings.SectionName}:{nameof(AuthenticationSettings.Authority)}",
                        keycloakAuthority),
                    new(
                        $"{AuthenticationSettings.SectionName}:{nameof(AuthenticationSettings.Audience)}",
                        KeycloakFixture.Audience)
                ]
            ));
            ReplaceOption(new ConnectionStringsSettings
                {
                    CentralDatabase = ServiceFixture.CentralDatabaseContainer.GetConnectionString(),
                    CompanyDatabase = ServiceFixture.CompanyDatabaseContainer.GetConnectionString()
                })
                .Invoke(host);
            ReplaceOption(
                    new StreamingSettings { BatchSize = 10 }
                )
                .Invoke(host);
        };
    }
}