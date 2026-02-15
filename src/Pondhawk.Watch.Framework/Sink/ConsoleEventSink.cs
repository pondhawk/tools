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

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pondhawk.Watch.Framework.Sink
{
    public class ConsoleEventSink : IEventSinkProvider
    {
        public void Start() { }
        public void Stop() { }

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
            var timestamp = le.Occurred.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var levelStr = GetLevelString((Level)le.Level);
            var indent = new string(' ', Math.Max(0, le.Nesting * 2));

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = GetConsoleColor((Level)le.Level);

            try
            {
                Console.WriteLine($"[{timestamp}] {levelStr,5} {indent}{le.Category}: {le.Title}");

                if (!string.IsNullOrEmpty(le.Payload))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    var payloadLines = le.Payload.Split('\n');
                    foreach (var line in payloadLines.Take(10))
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

        private static string GetLevelString(Level level)
        {
            switch (level)
            {
                case Level.Trace: return "TRACE";
                case Level.Debug: return "DEBUG";
                case Level.Info: return "INFO";
                case Level.Warning: return "WARN";
                case Level.Error: return "ERROR";
                default: return "???";
            }
        }

        private static ConsoleColor GetConsoleColor(Level level)
        {
            switch (level)
            {
                case Level.Trace: return ConsoleColor.DarkGray;
                case Level.Debug: return ConsoleColor.Gray;
                case Level.Info: return ConsoleColor.White;
                case Level.Warning: return ConsoleColor.Yellow;
                case Level.Error: return ConsoleColor.Red;
                default: return ConsoleColor.White;
            }
        }
    }
}
