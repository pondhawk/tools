using Serilog.Events;

namespace Pondhawk.Logging;

internal static class WatchSwitchConfig
{
    private static Func<string, LogEventLevel, bool>? _isEnabledFunc;

    internal static Func<string, LogEventLevel, bool>? IsEnabledFunc
    {
        get => Volatile.Read(ref _isEnabledFunc);
        set => Volatile.Write(ref _isEnabledFunc, value);
    }

    internal static bool IsEnabled(string category, LogEventLevel serilogLevel)
    {
        var func = IsEnabledFunc;
        return func is null || func(category, serilogLevel);
    }
}
