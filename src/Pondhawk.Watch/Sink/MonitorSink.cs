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

namespace Pondhawk.Watch.Sink;

/// <summary>
/// A test sink that accumulates log events for verification.
/// </summary>
/// <remarks>
/// <para>
/// When Accumulate is true, all received events are stored for later retrieval.
/// Use GetEvents() to retrieve accumulated events and Clear() to reset.
/// </para>
/// <para>
/// Thread-safety: All operations are thread-safe using ConcurrentBag.
/// </para>
/// </remarks>
public class MonitorSink : IEventSinkProvider
{
    private readonly ConcurrentBag<LogEvent> _events = new();

    /// <summary>
    /// Gets or sets whether to accumulate events.
    /// When false, events are discarded after Accept().
    /// </summary>
    public bool Accumulate { get; set; }

    /// <summary>
    /// Starts the monitor sink (no-op).
    /// </summary>
    public Task StartAsync() => Task.CompletedTask;

    /// <summary>
    /// Stops the monitor sink (no-op).
    /// </summary>
    public Task StopAsync() => Task.CompletedTask;

    /// <summary>
    /// Accepts a batch and optionally stores events if Accumulate is true.
    /// </summary>
    /// <param name="batch">The batch of events.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task Accept(LogEventBatch batch, CancellationToken cancellationToken = default)
    {
        if (Accumulate)
        {
            foreach (var e in batch.Events)
            {
                _events.Add(e);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Waits until at least the specified number of events have been accumulated.
    /// </summary>
    /// <param name="count">The minimum number of events to wait for.</param>
    /// <param name="timeout">Maximum time to wait (default: 2 seconds).</param>
    /// <returns>The accumulated events.</returns>
    public async Task<IReadOnlyList<LogEvent>> WaitForEventsAsync(int count = 1, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(2));
        while (_events.Count < count && DateTime.UtcNow < deadline)
            await Task.Delay(25);
        return _events.ToList();
    }

    /// <summary>
    /// Gets all accumulated events.
    /// </summary>
    /// <returns>A read-only list of accumulated events.</returns>
    public IReadOnlyList<LogEvent> GetEvents() => _events.ToList();

    /// <summary>
    /// Gets the count of accumulated events.
    /// </summary>
    public int Count => _events.Count;

    /// <summary>
    /// Clears all accumulated events.
    /// </summary>
    public void Clear() => _events.Clear();
}
