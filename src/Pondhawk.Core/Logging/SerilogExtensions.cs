using System.Runtime.CompilerServices;
using Pondhawk.Logging.Serializers;
using Serilog;
using Serilog.Events;

namespace Pondhawk.Logging;

/// <summary>
/// Extension methods on <see cref="Serilog.ILogger"/> and <see cref="object"/> for method tracing, object serialization, typed payloads, and logger creation.
/// </summary>
/// <remarks>
/// <para><c>GetLogger()</c> creates a Serilog <c>ILogger</c> with <c>SourceContext</c> set to the caller's type name.</para>
/// <para><c>EnterMethod()</c> creates a disposable scope that logs entry/exit with elapsed time at Verbose level.</para>
/// <para><c>LogObject()</c> serializes any object to JSON and attaches it as a structured payload.</para>
/// <para>Typed payload methods (<c>LogJson</c>, <c>LogSql</c>, <c>LogXml</c>, <c>LogYaml</c>, <c>LogText</c>)
/// attach content with syntax-type hints for Watch viewers.</para>
/// </remarks>
/// <example>
/// <code>
/// public class OrderService
/// {
///     public void ProcessOrder(Order order)
///     {
///         // Get a logger with SourceContext = "OrderService"
///         var logger = this.GetLogger();
///
///         // Method tracing with automatic entry/exit and elapsed time
///         using (this.EnterMethod())
///         {
///             logger.Information("Processing order {OrderId}", order.Id);
///             logger.LogObject(order);                    // Serialize order as JSON payload
///             logger.Inspect("Total", order.Total);       // Log "Total = 500.00"
///             logger.LogSql("Generated query", sqlText);  // Attach SQL payload
///         }
///     }
/// }
/// </code>
/// </example>
public static class SerilogExtensions
{
    private static readonly LoggerSource FallbackInstance = new();

    private static ILoggerSource? _default;
    public static ILoggerSource? Default
    {
        get => Volatile.Read(ref _default);
        set => Volatile.Write(ref _default, value);
    }

    #region Method Tracing

    public static MethodLogger EnterMethod(
        this ILogger logger,
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
    {
        var className = Path.GetFileNameWithoutExtension(file);
        var category = GetCategory(logger);
        var tracing = WatchSwitchConfig.IsEnabled(category, LogEventLevel.Verbose);

        if (tracing)
        {
            logger
                .ForContext(WatchPropertyNames.Nesting, 1)
                .Verbose("Entering {ClassName}.{Method}", className, method);
        }

        return new MethodLogger(logger, className, method, tracing);
    }

    #endregion

    #region Object Serialization

    public static void LogObject<T>(
        this ILogger logger,
        T value,
        LogEventLevel level = LogEventLevel.Debug)
    {
        var category = GetCategory(logger);
        if (!WatchSwitchConfig.IsEnabled(category, level))
            return;

        var (_, json) = JsonObjectSerializer.Instance.Serialize(value);
        var typeName = typeof(T).Name;

        logger
            .ForContext(WatchPropertyNames.PayloadType, (int)PayloadType.Json)
            .ForContext(WatchPropertyNames.PayloadContent, json)
            .Write(level, typeName);
    }

    public static void LogObject<T>(
        this ILogger logger,
        string title,
        T value,
        LogEventLevel level = LogEventLevel.Debug)
    {
        var category = GetCategory(logger);
        if (!WatchSwitchConfig.IsEnabled(category, level))
            return;

        var (_, json) = JsonObjectSerializer.Instance.Serialize(value);

        logger
            .ForContext(WatchPropertyNames.PayloadType, (int)PayloadType.Json)
            .ForContext(WatchPropertyNames.PayloadContent, json)
            .Write(level, title);
    }

    #endregion

    #region Typed Payloads

    public static void LogJson(
        this ILogger logger,
        string title,
        string? json,
        LogEventLevel level = LogEventLevel.Debug)
    {
        var category = GetCategory(logger);
        if (!WatchSwitchConfig.IsEnabled(category, level))
            return;

        logger
            .ForContext(WatchPropertyNames.PayloadType, (int)PayloadType.Json)
            .ForContext(WatchPropertyNames.PayloadContent, json ?? string.Empty)
            .Write(level, title);
    }

    public static void LogSql(
        this ILogger logger,
        string title,
        string? sql,
        LogEventLevel level = LogEventLevel.Debug)
    {
        var category = GetCategory(logger);
        if (!WatchSwitchConfig.IsEnabled(category, level))
            return;

        logger
            .ForContext(WatchPropertyNames.PayloadType, (int)PayloadType.Sql)
            .ForContext(WatchPropertyNames.PayloadContent, sql ?? string.Empty)
            .Write(level, title);
    }

    public static void LogXml(
        this ILogger logger,
        string title,
        string? xml,
        LogEventLevel level = LogEventLevel.Debug)
    {
        var category = GetCategory(logger);
        if (!WatchSwitchConfig.IsEnabled(category, level))
            return;

        logger
            .ForContext(WatchPropertyNames.PayloadType, (int)PayloadType.Xml)
            .ForContext(WatchPropertyNames.PayloadContent, xml ?? string.Empty)
            .Write(level, title);
    }

    public static void LogYaml(
        this ILogger logger,
        string title,
        string? yaml,
        LogEventLevel level = LogEventLevel.Debug)
    {
        var category = GetCategory(logger);
        if (!WatchSwitchConfig.IsEnabled(category, level))
            return;

        logger
            .ForContext(WatchPropertyNames.PayloadType, (int)PayloadType.Yaml)
            .ForContext(WatchPropertyNames.PayloadContent, yaml ?? string.Empty)
            .Write(level, title);
    }

    public static void LogText(
        this ILogger logger,
        string title,
        string? text,
        LogEventLevel level = LogEventLevel.Debug)
    {
        var category = GetCategory(logger);
        if (!WatchSwitchConfig.IsEnabled(category, level))
            return;

        logger
            .ForContext(WatchPropertyNames.PayloadType, (int)PayloadType.Text)
            .ForContext(WatchPropertyNames.PayloadContent, text ?? string.Empty)
            .Write(level, title);
    }

    #endregion

    public static void Inspect(
        this ILogger logger,
        string name,
        object? value,
        LogEventLevel level = LogEventLevel.Debug)
    {
        logger.Write(level, "{Name} = {Value}", name, value);
    }

    public static ILogger GetLogger(this object source)
    {
        return (Default ?? FallbackInstance).GetLogger(source);
    }

    public static MethodLogger EnterMethod(
        this object source,
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
    {
        return (Default ?? FallbackInstance).EnterMethod(source, method, file);
    }

    private static string GetCategory(ILogger logger)
    {
        // Serilog doesn't expose SourceContext directly from ILogger.
        // The category is set via ForContext<T>() or ForContext("SourceContext", value).
        // For extension method switch checks, we use a default category.
        // The sink will extract the actual SourceContext from the log event.
        return "Serilog";
    }
}
