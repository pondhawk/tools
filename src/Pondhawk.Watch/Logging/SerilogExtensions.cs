using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Events;

namespace Pondhawk.Watch;

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

    /// <summary>
    /// Gets or sets the default <see cref="ILoggerSource"/> used by <see cref="GetLogger"/> and EnterMethod.
    /// When <c>null</c>, a built-in <see cref="LoggerSource"/> is used.
    /// </summary>
    public static ILoggerSource? Default
    {
        get => Volatile.Read(ref _default);
        set => Volatile.Write(ref _default, value);
    }

#if NET7_0_OR_GREATER
    #region Method Tracing

    /// <summary>
    /// Creates a disposable method tracing scope that logs entry at Verbose level with <c>Watch.Nesting = 1</c>,
    /// and logs exit with elapsed time and <c>Watch.Nesting = -1</c> on dispose.
    /// </summary>
    /// <param name="logger">The Serilog logger to trace with.</param>
    /// <param name="method">The calling method name (auto-populated by compiler).</param>
    /// <param name="file">The calling file path, used to derive the class name (auto-populated by compiler).</param>
    /// <returns>A disposable <see cref="MethodLogger"/> that also implements <see cref="ILogger"/>.</returns>
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
#endif

    #region Object Serialization

    /// <summary>
    /// Serializes an object to JSON and logs it as a structured payload with the type name as the message.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="logger">The Serilog logger.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="level">The log event level. Defaults to <see cref="LogEventLevel.Debug"/>.</param>
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

    /// <summary>
    /// Serializes an object to JSON and logs it as a structured payload with a custom title.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="logger">The Serilog logger.</param>
    /// <param name="title">The log message title.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="level">The log event level. Defaults to <see cref="LogEventLevel.Debug"/>.</param>
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

    /// <summary>
    /// Logs a JSON string as a payload with <see cref="PayloadType.Json"/> syntax highlighting.
    /// </summary>
    /// <param name="logger">The Serilog logger.</param>
    /// <param name="title">The log message title.</param>
    /// <param name="json">The JSON content to attach.</param>
    /// <param name="level">The log event level. Defaults to <see cref="LogEventLevel.Debug"/>.</param>
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

    /// <summary>
    /// Logs a SQL string as a payload with <see cref="PayloadType.Sql"/> syntax highlighting.
    /// </summary>
    /// <param name="logger">The Serilog logger.</param>
    /// <param name="title">The log message title.</param>
    /// <param name="sql">The SQL content to attach.</param>
    /// <param name="level">The log event level. Defaults to <see cref="LogEventLevel.Debug"/>.</param>
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

    /// <summary>
    /// Logs an XML string as a payload with <see cref="PayloadType.Xml"/> syntax highlighting.
    /// </summary>
    /// <param name="logger">The Serilog logger.</param>
    /// <param name="title">The log message title.</param>
    /// <param name="xml">The XML content to attach.</param>
    /// <param name="level">The log event level. Defaults to <see cref="LogEventLevel.Debug"/>.</param>
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

    /// <summary>
    /// Logs a YAML string as a payload with <see cref="PayloadType.Yaml"/> syntax highlighting.
    /// </summary>
    /// <param name="logger">The Serilog logger.</param>
    /// <param name="title">The log message title.</param>
    /// <param name="yaml">The YAML content to attach.</param>
    /// <param name="level">The log event level. Defaults to <see cref="LogEventLevel.Debug"/>.</param>
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

    /// <summary>
    /// Logs a plain text string as a payload with <see cref="PayloadType.Text"/> type.
    /// </summary>
    /// <param name="logger">The Serilog logger.</param>
    /// <param name="title">The log message title.</param>
    /// <param name="text">The text content to attach.</param>
    /// <param name="level">The log event level. Defaults to <see cref="LogEventLevel.Debug"/>.</param>
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

    /// <summary>
    /// Logs a name/value pair as <c>"{Name} = {Value}"</c> at the specified level.
    /// </summary>
    /// <param name="logger">The Serilog logger.</param>
    /// <param name="name">The display name for the value.</param>
    /// <param name="value">The value to log.</param>
    /// <param name="level">The log event level. Defaults to <see cref="LogEventLevel.Debug"/>.</param>
    public static void Inspect(
        this ILogger logger,
        string name,
        object? value,
        LogEventLevel level = LogEventLevel.Debug)
    {
        logger.Write(level, "{Name} = {Value}", name, value);
    }

    /// <summary>
    /// Creates a Serilog <see cref="ILogger"/> with SourceContext set to the source object's type name.
    /// </summary>
    /// <param name="source">The object whose type name becomes the SourceContext.</param>
    /// <returns>A configured Serilog logger.</returns>
    public static ILogger GetLogger(this object source)
    {
        return (Default ?? FallbackInstance).GetLogger(source);
    }

#if NET7_0_OR_GREATER
    /// <summary>
    /// Creates a disposable method tracing scope for the calling object.
    /// Logs entry at Verbose level with <c>Watch.Nesting = 1</c> and exit with elapsed time on dispose.
    /// </summary>
    /// <param name="source">The object whose type provides the logger context.</param>
    /// <param name="method">The calling method name (auto-populated by compiler).</param>
    /// <param name="file">The calling file path, used to derive the class name (auto-populated by compiler).</param>
    /// <returns>A disposable <see cref="MethodLogger"/> that also implements <see cref="ILogger"/>.</returns>
    public static MethodLogger EnterMethod(
        this object source,
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
    {
        return (Default ?? FallbackInstance).EnterMethod(source, method, file);
    }
#endif

    private static string GetCategory(ILogger logger)
    {
        // Serilog doesn't expose SourceContext directly from ILogger.
        // The category is set via ForContext<T>() or ForContext("SourceContext", value).
        // For extension method switch checks, we use a default category.
        // The sink will extract the actual SourceContext from the log event.
        return "Serilog";
    }
}
