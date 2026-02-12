using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using Pondhawk.Logging;
using Serilog.Core;
using Serilog.Events;
using SerilogEvent = Serilog.Events.LogEvent;

namespace Pondhawk.Watch;

/// <summary>
/// Serilog ILogEventSink with Channel-based batching for the Watch pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Combines Channel-based batching (from WatchLoggerProvider) with
/// Serilog-to-Watch event mapping (from WatchBatchedSink).
/// </para>
/// <para>
/// Emit() is non-blocking: events are written to an unbounded channel.
/// A background task drains the channel by batch size or flush interval
/// and sends converted Watch LogEvents to the sink provider.
/// </para>
/// </remarks>
public sealed class WatchSink : ILogEventSink, IDisposable, IAsyncDisposable
{
    private readonly IEventSinkProvider _sink;
    private readonly ISwitchSource _switchSource;
    private readonly string _domain;
    private readonly int _batchSize;
    private readonly TimeSpan _flushInterval;

    private readonly Channel<SerilogEvent> _channel;
    private readonly Task _flushTask;
    private readonly TaskCompletionSource _flushCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _disposed;

    public WatchSink(
        IEventSinkProvider sink,
        ISwitchSource switchSource,
        string domain = "Default",
        int batchSize = 100,
        TimeSpan? flushInterval = null)
    {
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        _switchSource = switchSource ?? throw new ArgumentNullException(nameof(switchSource));
        _domain = domain ?? throw new ArgumentNullException(nameof(domain));
        _batchSize = batchSize;
        _flushInterval = flushInterval ?? TimeSpan.FromMilliseconds(100);

        _channel = Channel.CreateUnbounded<SerilogEvent>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        });

        _flushTask = Task.Run(FlushLoopAsync);
    }

    /// <summary>
    /// Emits a Serilog log event into the channel for batched processing.
    /// </summary>
    public void Emit(SerilogEvent logEvent)
    {
        if (_disposed)
            return;

        _channel.Writer.TryWrite(logEvent);
    }

    private async Task FlushLoopAsync()
    {
        var batch = new List<SerilogEvent>(_batchSize);
        var reader = _channel.Reader;

        try
        {
            while (true)
            {
                batch.Clear();

                if (!await reader.WaitToReadAsync())
                    break;

                using var timeoutCts = new CancellationTokenSource(_flushInterval);

                try
                {
                    while (batch.Count < _batchSize)
                    {
                        if (reader.TryRead(out var logEvent))
                        {
                            batch.Add(logEvent);
                        }
                        else if (!await reader.WaitToReadAsync(timeoutCts.Token))
                        {
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout expired, flush what we have
                }

                if (batch.Count > 0)
                {
                    await FlushBatchAsync(batch);
                }
            }
        }
        finally
        {
            _flushCompleted.TrySetResult();
        }
    }

    private async Task FlushBatchAsync(List<SerilogEvent> events)
    {
        var watchBatch = new LogEventBatch { Domain = _domain };

        foreach (var serilogEvent in events)
        {
            var converted = ConvertEvent(serilogEvent);
            if (converted is not null)
                watchBatch.Events.Add(converted);
        }

        if (watchBatch.Events.Count > 0)
        {
            try
            {
                await _sink.Accept(watchBatch, CancellationToken.None);
            }
            catch
            {
                // Sink failures are handled by the sink itself (circuit breaker, etc.)
            }
        }
    }

    private LogEvent? ConvertEvent(SerilogEvent serilogEvent)
    {
        var category = GetCategory(serilogEvent);
        var sw = _switchSource.Lookup(category);

        if (sw.IsQuiet)
            return null;

        if (serilogEvent.Level < sw.Level)
            return null;

        var le = new LogEvent
        {
            Category = category,
            Level = (int)serilogEvent.Level,
            Color = sw.Color.ToArgb(),
            Tag = sw.Tag,
            Title = serilogEvent.RenderMessage(),
            CorrelationId = GetCorrelationId(),
            Occurred = serilogEvent.Timestamp.UtcDateTime
        };

        // Extract Watch-specific properties
        if (serilogEvent.Properties.TryGetValue(WatchPropertyNames.Nesting, out var nestingValue) &&
            nestingValue is ScalarValue { Value: int nesting })
        {
            le.Nesting = nesting;
        }

        if (serilogEvent.Properties.TryGetValue(WatchPropertyNames.PayloadType, out var typeValue) &&
            typeValue is ScalarValue { Value: int payloadType } &&
            serilogEvent.Properties.TryGetValue(WatchPropertyNames.PayloadContent, out var contentValue) &&
            contentValue is ScalarValue { Value: string payloadContent })
        {
            le.Type = payloadType;
            le.Payload = payloadContent;
        }
        else if (serilogEvent.Exception is not null)
        {
            le.ErrorType = serilogEvent.Exception.GetType().FullName ?? serilogEvent.Exception.GetType().Name;
            le.Error = serilogEvent.Exception;
        }
        else
        {
            var payload = BuildStructuredPayload(serilogEvent);
            if (payload is not null)
            {
                le.Type = (int)PayloadType.Json;
                le.Payload = payload;
            }
        }

        return le;
    }

    private static string GetCategory(SerilogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext) &&
            sourceContext is ScalarValue { Value: string category })
        {
            return category;
        }

        return "Serilog";
    }

    private static string GetCorrelationId()
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            var correlation = activity.GetBaggageItem(Pondhawk.Logging.CorrelationManager.BaggageKey);
            if (!string.IsNullOrEmpty(correlation))
                return correlation;

            var newId = Ulid.NewUlid().ToString();
            activity.SetBaggage(Pondhawk.Logging.CorrelationManager.BaggageKey, newId);
            return newId;
        }

        return Ulid.NewUlid().ToString();
    }

    private static string? BuildStructuredPayload(SerilogEvent logEvent)
    {
        var properties = logEvent.Properties
            .Where(p => p.Key != "SourceContext" &&
                        !p.Key.StartsWith("Watch.", StringComparison.Ordinal))
            .ToList();

        if (properties.Count == 0)
            return null;

        try
        {
            var dict = new Dictionary<string, object?>();
            foreach (var prop in properties)
            {
                dict[prop.Key] = ConvertPropertyValue(prop.Value);
            }

            return JsonSerializer.Serialize(dict);
        }
        catch
        {
            return null;
        }
    }

    private static object? ConvertPropertyValue(LogEventPropertyValue value)
    {
        return value switch
        {
            ScalarValue sv => sv.Value,
            SequenceValue seq => seq.Elements.Select(ConvertPropertyValue).ToList(),
            StructureValue str => str.Properties.ToDictionary(p => p.Name, p => ConvertPropertyValue(p.Value)),
            DictionaryValue dv => dv.Elements.ToDictionary(
                kvp => ConvertPropertyValue(kvp.Key)?.ToString() ?? "",
                kvp => ConvertPropertyValue(kvp.Value)),
            _ => value.ToString()
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _channel.Writer.Complete();

        try
        {
            _flushCompleted.Task.Wait(TimeSpan.FromSeconds(10));
        }
        catch
        {
            // Ignore exceptions during disposal
        }

        try
        {
            _sink.StopAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Ignore exceptions during sink stop
        }

        try
        {
            _switchSource.StopAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Ignore exceptions during switch source stop
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        _channel.Writer.Complete();

        try
        {
            await _flushCompleted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        catch
        {
            // Ignore exceptions during disposal (including timeout)
        }

        try
        {
            await _sink.StopAsync();
        }
        catch
        {
            // Ignore exceptions during sink stop
        }

        try
        {
            await _switchSource.StopAsync();
        }
        catch
        {
            // Ignore exceptions during switch source stop
        }
    }
}
