/*
The MIT License (MIT)

Copyright (c) 2024 Pond Hawk Technologies Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Serilog.Events;
using Pondhawk.Watch.Sink;

namespace Pondhawk.Watch.Http;

/// <summary>
/// An event sink provider that sends log event batches to a Watch Server over HTTP.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// - Circuit breaker: Opens after N consecutive failures, re-tries after delay
/// - Critical event buffer: Buffers Warning/Error events during outage
/// - Exponential backoff: Increases delay between retries up to max
/// - MemoryPack + Brotli compression: Efficient wire format
/// </para>
/// <para>
/// Thread-safety: All operations are thread-safe.
/// </para>
/// </remarks>
public class HttpEventSinkProvider : IEventSinkProvider
{
    private readonly HttpClient _client;
    private readonly string _domain;

    // Circuit breaker state
    private int _consecutiveFailures;
    private DateTime _circuitOpenUntil = DateTime.MinValue;
    private readonly object _circuitLock = new();

    // Critical event buffer
    private readonly ConcurrentQueue<LogEvent> _criticalBuffer = new();

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

    private long _droppedEventCount;

    /// <summary>
    /// Creates a new HttpEventSinkProvider.
    /// </summary>
    /// <param name="client">The HTTP client configured for the Watch Server.</param>
    /// <param name="domain">The domain name for log events.</param>
    public HttpEventSinkProvider(HttpClient client, string domain)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _domain = domain ?? throw new ArgumentNullException(nameof(domain));
    }

    /// <summary>
    /// Starts the sink provider.
    /// </summary>
    public Task StartAsync() => Task.CompletedTask;

    /// <summary>
    /// Stops the sink provider.
    /// </summary>
    public Task StopAsync() => Task.CompletedTask;

    /// <summary>
    /// Accepts a batch of log events and sends to the server.
    /// </summary>
    /// <param name="batch">The batch of events.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task Accept(LogEventBatch batch, CancellationToken cancellationToken = default)
    {
        // Check circuit state
        if (IsCircuitOpen)
        {
            // Buffer critical events
            BufferCriticalEvents(batch);
            return;
        }

        try
        {
            // Add any buffered critical events to this batch
            FlushCriticalBuffer(batch);

            await SendBatchAsync(batch, cancellationToken);
            OnSuccess();
        }
        catch
        {
            OnFailure(batch);
        }
    }

    private async Task SendBatchAsync(LogEventBatch batch, CancellationToken ct)
    {
        // Serialize with MemoryPack + Brotli
        await using var stream = await LogEventBatchSerializer.ToStream(batch);

        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.Add("X-Domain", _domain);

        var response = await _client.PostAsync("api/sink", content, ct);
        response.EnsureSuccessStatusCode();
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
}
