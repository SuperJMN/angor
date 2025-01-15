namespace Angor.Test.Suppa;

using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class XUnitLogger : ILogger
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly string _categoryName;

    public XUnitLogger(ITestOutputHelper outputHelper, string categoryName)
    {
        _outputHelper = outputHelper ?? 
            throw new ArgumentNullException(nameof(outputHelper));
        _categoryName = categoryName;
    }

    public bool IsEnabled(LogLevel logLevel) => true; // Ajusta si quieres filtrar.

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        _outputHelper.WriteLine($"{DateTime.Now:HH:mm:ss} [{logLevel}] {_categoryName}: {message}");

        if (exception is not null)
        {
            _outputHelper.WriteLine(exception.ToString());
        }
    }

    public IDisposable BeginScope<TState>(TState state) => default!;
}

public class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _outputHelper;

    public XUnitLoggerProvider(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper ?? 
            throw new ArgumentNullException(nameof(outputHelper));
    }

    public ILogger CreateLogger(string categoryName)
        => new XUnitLogger(_outputHelper, categoryName);

    public void Dispose() { }
}