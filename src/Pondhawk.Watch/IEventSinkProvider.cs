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

namespace Pondhawk.Watch;

/// <summary>
/// Provides the interface for log event sinks that receive and process batches of log events.
/// </summary>
/// <remarks>
/// <para>
/// Implementations include:
/// - ConsoleEventSink: Writes events to the console with formatting
/// - HttpEventSinkProvider: Posts batches to a Watch server
/// - RealtimeSink: Streams to SmartInspect for local debugging
/// - MonitorSink: Accumulates events for testing
/// </para>
/// <para>
/// Thread-safety: Accept() may be called concurrently from multiple batch processors.
/// Implementations must handle concurrent access appropriately.
/// </para>
/// </remarks>
public interface IEventSinkProvider
{
    /// <summary>
    /// Starts the sink provider. Called once during initialization.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartAsync();

    /// <summary>
    /// Stops the sink provider and releases resources.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopAsync();

    /// <summary>
    /// Accepts a batch of log events for processing.
    /// </summary>
    /// <param name="batch">The batch of events to process.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Implementations should handle failures gracefully:
    /// - HttpEventSinkProvider uses circuit breaker and buffers critical events
    /// - Console sinks should not throw on write failures
    /// - Test sinks may throw to surface test failures
    /// </remarks>
    Task Accept(LogEventBatch batch, CancellationToken cancellationToken = default);
}
