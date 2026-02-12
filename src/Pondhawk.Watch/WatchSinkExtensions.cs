using Pondhawk.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Pondhawk.Watch;

/// <summary>
/// Serilog configuration extensions for the Watch sink.
/// </summary>
public static class WatchSinkExtensions
{
    /// <summary>
    /// Adds a Watch sink to the Serilog configuration with Channel-based batching.
    /// </summary>
    /// <param name="config">The Serilog sink configuration.</param>
    /// <param name="sink">The Watch event sink provider.</param>
    /// <param name="switchSource">The switch source for log level control.</param>
    /// <param name="domain">The domain name for log event batches.</param>
    /// <param name="batchSize">The batch size before flushing.</param>
    /// <param name="flushInterval">The flush interval when batch size is not reached.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration WatchSink(
        this LoggerSinkConfiguration config,
        IEventSinkProvider sink,
        ISwitchSource switchSource,
        string domain = "Default",
        int batchSize = 100,
        TimeSpan? flushInterval = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(sink);
        ArgumentNullException.ThrowIfNull(switchSource);

        WatchSwitchConfig.IsEnabledFunc = (category, serilogLevel) =>
        {
            var sw = switchSource.Lookup(category);
            if (sw.IsQuiet)
                return false;

            return serilogLevel >= sw.Level;
        };

        SerilogExtensions.Default = new LoggerSource();

        switchSource.StartAsync().GetAwaiter().GetResult();
        sink.StartAsync().GetAwaiter().GetResult();

        var watchSink = new WatchSink(sink, switchSource, domain, batchSize, flushInterval);

        return config.Sink(watchSink);
    }
}
