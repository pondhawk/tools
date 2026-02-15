using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Pondhawk.Logging;

/// <summary>
/// A disposable ILogger wrapper returned by EnterMethod.
/// Delegates all ILogger methods to the inner logger and logs method exit on dispose.
/// </summary>
/// <remarks>
/// Level-specific methods (Verbose, Debug, Information, Warning, Error, Fatal)
/// are provided by Serilog's default interface implementations, which delegate to Write().
/// </remarks>
public sealed class MethodLogger : ILogger, IDisposable
{
    private readonly ILogger _logger;
    private readonly string _className;
    private readonly string _method;
    private readonly long _startTimestamp;
    private readonly bool _tracing;
    private bool _disposed;

    internal MethodLogger(ILogger logger, string className, string method, bool tracing)
    {
        _logger = logger;
        _className = className;
        _method = method;
        _startTimestamp = Stopwatch.GetTimestamp();
        _tracing = tracing;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_tracing && WatchSwitchConfig.IsEnabled("Serilog", LogEventLevel.Verbose))
        {
            var elapsed = Stopwatch.GetElapsedTime(_startTimestamp);
            _logger
                .ForContext(WatchPropertyNames.Nesting, -1)
                .Verbose("Exiting {ClassName}.{Method} ({Elapsed:F2}ms)", _className, _method, elapsed.TotalMilliseconds);
        }
    }

    // ILogger — context methods
    public ILogger ForContext(ILogEventEnricher enricher) => _logger.ForContext(enricher);
    public ILogger ForContext(IEnumerable<ILogEventEnricher> enrichers) => _logger.ForContext(enrichers);
    public ILogger ForContext(string propertyName, object? value, bool destructureObjects = false) => _logger.ForContext(propertyName, value, destructureObjects);
    public ILogger ForContext<TSource>() => _logger.ForContext<TSource>();
    public ILogger ForContext(Type source) => _logger.ForContext(source);

    // ILogger — core write methods
    public void Write(LogEvent logEvent) => _logger.Write(logEvent);
    public void Write(LogEventLevel level, string messageTemplate) => _logger.Write(level, messageTemplate);
    public void Write<T>(LogEventLevel level, string messageTemplate, T propertyValue) => _logger.Write(level, messageTemplate, propertyValue);
    public void Write<T0, T1>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1) => _logger.Write(level, messageTemplate, propertyValue0, propertyValue1);
    public void Write<T0, T1, T2>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) => _logger.Write(level, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    public void Write(LogEventLevel level, string messageTemplate, params object?[]? propertyValues) => _logger.Write(level, messageTemplate, propertyValues);
    public void Write(LogEventLevel level, Exception? exception, string messageTemplate) => _logger.Write(level, exception, messageTemplate);
    public void Write<T>(LogEventLevel level, Exception? exception, string messageTemplate, T propertyValue) => _logger.Write(level, exception, messageTemplate, propertyValue);
    public void Write<T0, T1>(LogEventLevel level, Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1) => _logger.Write(level, exception, messageTemplate, propertyValue0, propertyValue1);
    public void Write<T0, T1, T2>(LogEventLevel level, Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) => _logger.Write(level, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    public void Write(LogEventLevel level, Exception? exception, string messageTemplate, params object?[]? propertyValues) => _logger.Write(level, exception, messageTemplate, propertyValues);

    // ILogger — utility methods
    public bool IsEnabled(LogEventLevel level) => _logger.IsEnabled(level);
    public bool BindMessageTemplate(string messageTemplate, object?[]? propertyValues, [NotNullWhen(true)] out MessageTemplate? parsedTemplate, [NotNullWhen(true)] out IEnumerable<LogEventProperty>? boundProperties) => _logger.BindMessageTemplate(messageTemplate, propertyValues, out parsedTemplate, out boundProperties);
    public bool BindProperty(string? propertyName, object? value, bool destructureObjects, [NotNullWhen(true)] out LogEventProperty? property) => _logger.BindProperty(propertyName, value, destructureObjects, out property);
}
