using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;

public delegate void LogMessage(LogLevel logLevel,
    string categoryName,
    EventId eventId,
    string message,
    Exception? exception);

public class GrpcServiceTestFixture<TProgram> : IDisposable, IAsyncLifetime where TProgram : class
{
    private TestServer? _server;
    private Action<IWebHostBuilder>? _configureWebHost;
    private HttpMessageHandler? _handler;

    internal PostgreSqlContainer CentralDatabaseContainer;
    internal PostgreSqlContainer CompanyDatabaseContainer;

    public event LogMessage? LoggedMessage;

    public GrpcServiceTestFixture()
    {
        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) =>
        {
            LoggedMessage?.Invoke(logLevel, category, eventId, message, exception);
        }));
    }

    private void SetupServer()
    {
        var appFactory = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => { services.AddSingleton<ILoggerFactory>(LoggerFactory); });
                _configureWebHost?.Invoke(builder);
            });
        _server = appFactory.Server;
        _handler = _server.CreateHandler();
    }

    public HttpMessageHandler Handler
    {
        get
        {
            if (_handler is null)
                SetupServer();
            return _handler!;
        }
    }


    public void ConfigureWebHost(Action<IWebHostBuilder> configure)
    {
        _configureWebHost = configure;
    }

    public LoggerFactory LoggerFactory { get; }

    public void Dispose()
    {
        Handler.Dispose();
        _server?.Dispose();
        GC.SuppressFinalize(this);
    }

    internal GrpcTestContext<TProgram> GetTestContext(ITestOutputHelper outputHelper)
        => new(this, outputHelper);

    public async Task InitializeAsync()
    {
        CentralDatabaseContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17.6")
            .Build();
        CompanyDatabaseContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17.6")
            .Build();

        await CentralDatabaseContainer.StartAsync();
        await CompanyDatabaseContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }
}