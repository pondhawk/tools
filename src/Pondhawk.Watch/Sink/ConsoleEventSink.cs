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

using Serilog.Events;

namespace Pondhawk.Watch.Sink;

/// <summary>
/// A simple event sink that writes log events to the console with color formatting.
/// </summary>
/// <remarks>
/// <para>
/// Output is formatted as: [Timestamp] LEVEL Category: Title
/// Events with payloads are followed by an indented payload section.
/// </para>
/// <para>
/// Thread-safety: Accept() can be called concurrently, but console output may interleave.
/// </para>
/// </remarks>
public class ConsoleEventSink : IEventSinkProvider
{
    /// <summary>
    /// Starts the console sink (no-op).
    /// </summary>
    public Task StartAsync() => Task.CompletedTask;

    /// <summary>
    /// Stops the console sink (no-op).
    /// </summary>
    public Task StopAsync() => Task.CompletedTask;

    /// <summary>
    /// Accepts a batch and writes each event to the console.
    /// </summary>
    /// <param name="batch">The batch of events to write.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task Accept(LogEventBatch batch, CancellationToken cancellationToken = default)
    {
        foreach (var le in batch.Events)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            WriteEvent(le);
        }

        return Task.CompletedTask;
    }

    private static void WriteEvent(LogEvent le)
    {
        var timestamp = le.Occurred.ToString("HH:mm:ss.fff");
        var levelStr = GetLevelString((LogEventLevel)le.Level);
        var indent = new string(' ', Math.Max(0, le.Nesting * 2));

        // Set console color based on level
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = GetConsoleColor((LogEventLevel)le.Level);

        try
        {
            Console.WriteLine($"[{timestamp}] {levelStr,5} {indent}{le.Category}: {le.Title}");

            if (!string.IsNullOrEmpty(le.Payload))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                var payloadLines = le.Payload.Split('\n');
                foreach (var line in payloadLines.Take(10)) // Limit payload lines
                {
                    Console.WriteLine($"                    {line.TrimEnd()}");
                }

                if (payloadLines.Length > 10)
                {
                    Console.WriteLine($"                    ... ({payloadLines.Length - 10} more lines)");
                }
            }
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }

    private static string GetLevelString(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => "TRACE",
        LogEventLevel.Debug => "DEBUG",
        LogEventLevel.Information => "INFO",
        LogEventLevel.Warning => "WARN",
        LogEventLevel.Error => "ERROR",
        LogEventLevel.Fatal => "FATAL",
        _ => "???"
    };

    private static ConsoleColor GetConsoleColor(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => ConsoleColor.DarkGray,
        LogEventLevel.Debug => ConsoleColor.Gray,
        LogEventLevel.Information => ConsoleColor.White,
        LogEventLevel.Warning => ConsoleColor.Yellow,
        LogEventLevel.Error => ConsoleColor.Red,
        LogEventLevel.Fatal => ConsoleColor.DarkRed,
        _ => ConsoleColor.White
    };
}
