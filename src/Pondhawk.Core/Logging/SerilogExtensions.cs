using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Pondhawk.Logging.Serializers;
using Pondhawk.Logging.Utilities;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Pondhawk.Logging;

public static class SerilogExtensions
{
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
        var category = source.GetType().GetConciseFullName();
        return Log.ForContext(Constants.SourceContextPropertyName, category);
    }

    public static MethodLogger EnterMethod(
        this object source,
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
    {
        return source.GetLogger().EnterMethod(method, file);
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
