using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;

public abstract class IntegrationTestBase : IClassFixture<GrpcServiceTestFixture<Program>>, IAsyncLifetime
{
    internal readonly GrpcTestContext<Program> Context;
    private GrpcChannel? _channel;

    protected readonly GrpcServiceTestFixture<Program> Fixture;
    private readonly Action<IWebHostBuilder> _hostConfiguration;

    protected IntegrationTestBase(GrpcServiceTestFixture<Program> serviceTestFixture, ITestOutputHelper outputHelper)
    {
        Context = serviceTestFixture.GetTestContext(outputHelper);
        Fixture = serviceTestFixture;
        _hostConfiguration = host =>
        {
            ReplaceOption(new ConnectionStringsSettings
                {
                    CentralDatabase = serviceTestFixture.CentralDatabaseContainer.GetConnectionString(),
                    CompanyDatabase = serviceTestFixture.CompanyDatabaseContainer.GetConnectionString()
                })
                .Invoke(host);
            ReplaceOption(
                    new StreamingSettings { BatchSize = 10 }
                )
                .Invoke(host);
        };
        serviceTestFixture.ConfigureWebHost(_hostConfiguration);
    }

    protected void AddAdditionalHostConfiguration(Action<IWebHostBuilder> additionalHostConfiguration)
    {
        Fixture.ConfigureWebHost(host =>
        {
            _hostConfiguration(host);
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

    private GrpcChannel CreateChannel() =>
        GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            LoggerFactory = Fixture.LoggerFactory,
            HttpHandler = Fixture.Handler
        });


    public Task InitializeAsync()
        => Task.WhenAll(
            ApplyInitScript(Fixture.CentralDatabaseContainer.GetConnectionString(), "central.sql"),
            ApplyInitScript(Fixture.CompanyDatabaseContainer.GetConnectionString(), "company.sql")
        );

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

    private Task ResetDatabases()
        => Task.WhenAll(
            DropDatabaseSchema(Fixture.CompanyDatabaseContainer.GetConnectionString()),
            DropDatabaseSchema(Fixture.CentralDatabaseContainer.GetConnectionString())
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
}