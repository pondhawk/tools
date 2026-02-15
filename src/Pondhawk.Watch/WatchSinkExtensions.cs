using CommunityToolkit.Diagnostics;
using Serilog;
using Serilog.Configuration;

namespace Pondhawk.Watch;

/// <summary>
/// Serilog configuration extensions for the Watch sink.
/// </summary>
public static class WatchSinkExtensions
{
    /// <summary>
    /// Adds a Watch sink using just a server URL and domain.
    /// Creates the HttpClient and WatchSwitchSource internally.
    /// </summary>
    /// <param name="config">The Serilog sink configuration.</param>
    /// <param name="serverUrl">The Watch Server URL.</param>
    /// <param name="domain">The domain name for log event batches.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration AddWatch(
        this LoggerSinkConfiguration config,
        string serverUrl,
        string domain)
    {
        return AddWatch(config, serverUrl, domain, _ => { });
    }

    /// <summary>
    /// Adds a Watch sink using a server URL, domain, and options customization.
    /// Creates the HttpClient and WatchSwitchSource internally.
    /// </summary>
    /// <param name="config">The Serilog sink configuration.</param>
    /// <param name="serverUrl">The Watch Server URL.</param>
    /// <param name="domain">The domain name for log event batches.</param>
    /// <param name="configure">An action to customize the sink options.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration AddWatch(
        this LoggerSinkConfiguration config,
        string serverUrl,
        string domain,
        Action<WatchSinkOptions> configure)
    {
        Guard.IsNotNull(config);
        Guard.IsNotNullOrWhiteSpace(serverUrl);
        Guard.IsNotNullOrWhiteSpace(domain);
        Guard.IsNotNull(configure);

        var options = new WatchSinkOptions { ServerUrl = serverUrl, Domain = domain };
        configure(options);

        var httpClient = new HttpClient { BaseAddress = new Uri(options.ServerUrl) };
        var switchSource = new WatchSwitchSource(httpClient, options.Domain, options.PollInterval);
        switchSource.WhenNotMatched(options.DefaultLevel, options.DefaultColor);

        return WatchSink(config, httpClient, switchSource, options.Domain, options.BatchSize, options.FlushInterval);
    }

    /// <summary>
    /// Adds a Watch sink to the Serilog configuration with Channel-based batching.
    /// This is the low-level API for advanced scenarios where you manage the HttpClient
    /// and SwitchSource yourself.
    /// </summary>
    /// <param name="config">The Serilog sink configuration.</param>
    /// <param name="httpClient">The HTTP client configured for the Watch Server.</param>
    /// <param name="switchSource">The switch source for log level control.</param>
    /// <param name="domain">The domain name for log event batches.</param>
    /// <param name="batchSize">The batch size before flushing.</param>
    /// <param name="flushInterval">The flush interval when batch size is not reached.</param>
    /// <returns>The logger configuration for chaining.</returns>
    public static LoggerConfiguration WatchSink(
        this LoggerSinkConfiguration config,
        HttpClient httpClient,
        SwitchSource switchSource,
        string domain = "Default",
        int batchSize = 100,
        TimeSpan? flushInterval = null)
    {
        Guard.IsNotNull(config);
        Guard.IsNotNull(httpClient);
        Guard.IsNotNull(switchSource);

        switchSource.Start();

        var watchSink = new WatchSink(httpClient, switchSource, domain, batchSize, flushInterval);

        return config.Sink(watchSink);
    }
}
