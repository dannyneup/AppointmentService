using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;

internal class GrpcTestContext<TProgram> : IDisposable where TProgram : class
{
    private readonly Stopwatch _stopwatch;
    private readonly GrpcServiceTestFixture<TProgram> _fixture;
    private readonly ITestOutputHelper _outputHelper;

    public GrpcTestContext(GrpcServiceTestFixture<TProgram> fixture, ITestOutputHelper outputHelper)
    {
        _stopwatch = Stopwatch.StartNew();
        _fixture = fixture;
        _outputHelper = outputHelper;
        _fixture.LoggedMessage += WriteMessage;
    }

    private void WriteMessage(LogLevel logLevel, string category, EventId eventId, string message, Exception? exception)
    {
        var log = $"{_stopwatch.Elapsed.TotalSeconds:N3}s {category} - {logLevel}: {message}";
        if (exception != null)
        {
            log += Environment.NewLine + exception;
        }
        _outputHelper.WriteLine(log);
    }

    public void Dispose()
    {
        _fixture.LoggedMessage -= WriteMessage;
    }
}