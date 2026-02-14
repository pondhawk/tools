using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Channels;
using CommunityToolkit.Diagnostics;
using Pondhawk.Logging;
using Serilog.Core;
using Serilog.Events;
using SerilogEvent = Serilog.Events.LogEvent;

namespace Pondhawk.Watch;

/// <summary>
/// Serilog ILogEventSink with Channel-based batching, HTTP posting, and circuit breaker
/// for the Watch pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Combines Channel-based batching with Serilog-to-Watch event mapping and
/// HTTP posting with circuit breaker resilience.
/// </para>
/// <para>
/// Emit() is non-blocking: events are written to an unbounded channel.
/// A background task drains the channel by batch size or flush interval
/// and sends converted Watch LogEvents to the Watch Server.
/// </para>
/// </remarks>
public sealed class WatchSink : ILogEventSink, IDisposable, IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly SwitchSource _switchSource;
    private readonly string _domain;
    private readonly int _batchSize;
    private readonly TimeSpan _flushInterval;

    private readonly Channel<SerilogEvent> _channel;
    private readonly Task _flushTask;
    private readonly TaskCompletionSource _flushCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _disposed;

    // Circuit breaker state
    private int _consecutiveFailures;
    private DateTime _circuitOpenUntil = DateTime.MinValue;
    private readonly object _circuitLock = new();

    // Critical event buffer
    private readonly ConcurrentQueue<LogEvent> _criticalBuffer = new();
    private long _droppedEventCount;

    /// <summary>
    /// Gets or sets the failure threshold before the circuit opens.
    /// </summary>
    public int FailureThreshold { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay before retrying after circuit opens.
    /// </summary>
    public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the maximum retry delay.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Gets or sets the maximum number of critical events to buffer during outage.
    /// </summary>
    public int MaxCriticalBufferSize { get; set; } = 1000;

    /// <summary>
    /// Gets the current circuit state.
    /// </summary>
    public bool IsCircuitOpen
    {
        get
        {
            lock (_circuitLock)
            {
                return _circuitOpenUntil > DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Gets the number of events currently in the critical buffer.
    /// </summary>
    public int CriticalBufferCount => _criticalBuffer.Count;

    /// <summary>
    /// Gets the total number of events dropped due to buffer overflow.
    /// </summary>
    public long DroppedEventCount => Interlocked.Read(ref _droppedEventCount);

    public WatchSink(
        HttpClient client,
        SwitchSource switchSource,
        string domain = "Default",
        int batchSize = 100,
        TimeSpan? flushInterval = null)
    {
        Guard.IsNotNull(client);
        Guard.IsNotNull(switchSource);
        Guard.IsNotNull(domain);

        _client = client;
        _switchSource = switchSource;
        _domain = domain;
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
        if (Volatile.Read(ref _disposed) != 0)
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

    internal async Task FlushBatchAsync(List<SerilogEvent> events)
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
            await SendBatchAsync(watchBatch);
        }
    }

    private async Task SendBatchAsync(LogEventBatch batch)
    {
        // Check circuit state
        if (IsCircuitOpen)
        {
            BufferCriticalEvents(batch);
            return;
        }

        try
        {
            // Add any buffered critical events to this batch
            FlushCriticalBuffer(batch);

            await using var stream = await LogEventBatchSerializer.ToStream(batch);

            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Headers.Add("X-Domain", _domain);

            var response = await _client.PostAsync("api/sink", content, CancellationToken.None);
            response.EnsureSuccessStatusCode();

            OnSuccess();
        }
        catch
        {
            OnFailure(batch);
        }
    }

    private void OnSuccess()
    {
        lock (_circuitLock)
        {
            _consecutiveFailures = 0;
            _circuitOpenUntil = DateTime.MinValue;
        }
    }

    private void OnFailure(LogEventBatch batch)
    {
        lock (_circuitLock)
        {
            _consecutiveFailures++;

            if (_consecutiveFailures >= FailureThreshold)
            {
                // Calculate delay with exponential backoff
                var backoffFactor = Math.Pow(2, _consecutiveFailures - FailureThreshold);
                var delay = TimeSpan.FromTicks((long)(BaseRetryDelay.Ticks * backoffFactor));

                if (delay > MaxRetryDelay)
                    delay = MaxRetryDelay;

                _circuitOpenUntil = DateTime.UtcNow.Add(delay);
            }
        }

        // Buffer critical events from the failed batch
        BufferCriticalEvents(batch);
    }

    private void BufferCriticalEvents(LogEventBatch batch)
    {
        foreach (var e in batch.Events)
        {
            // Buffer Warning and Error level events
            if (e.Level >= (int)LogEventLevel.Warning)
            {
                // Enforce max buffer size - track dropped events
                while (_criticalBuffer.Count >= MaxCriticalBufferSize)
                {
                    if (_criticalBuffer.TryDequeue(out _))
                    {
                        Interlocked.Increment(ref _droppedEventCount);
                    }
                }

                _criticalBuffer.Enqueue(e);
            }
        }
    }

    private void FlushCriticalBuffer(LogEventBatch batch)
    {
        while (_criticalBuffer.TryDequeue(out var e))
        {
            batch.Events.Insert(0, e);
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
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        _channel.Writer.Complete();

        try
        {
            _flushCompleted.Task.Wait(TimeSpan.FromSeconds(10));
        }
        catch
        {
            // Ignore exceptions during disposal
        }

        _switchSource.Stop();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        _channel.Writer.Complete();

        try
        {
            await _flushCompleted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        catch
        {
            // Ignore exceptions during disposal (including timeout)
        }

        _switchSource.Stop();
    }
}
