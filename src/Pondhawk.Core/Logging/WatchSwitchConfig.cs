using Serilog.Events;

namespace Pondhawk.Logging;

internal static class WatchSwitchConfig
{
    internal static Func<string, LogEventLevel, bool>? IsEnabledFunc;

    internal static bool IsEnabled(string category, LogEventLevel serilogLevel)
    {
        var func = IsEnabledFunc;
        return func is null || func(category, serilogLevel);
    }
}
