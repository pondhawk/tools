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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Pondhawk.Watch.Framework.Serializers;

namespace Pondhawk.Watch.Framework
{
    public sealed class WatchLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, WatchLogger> _loggers = new ConcurrentDictionary<string, WatchLogger>();
        private readonly ConcurrentQueue<LogEvent> _queue = new ConcurrentQueue<LogEvent>();
        private readonly Timer _flushTimer;
        private int _flushing;
        private bool _disposed;

        public ISwitchSource SwitchSource { get; }
        public IEventSinkProvider Sink { get; }
        public IObjectSerializer ObjectSerializer { get; }
        public string Domain { get; set; } = "Default";
        public int BatchSize { get; set; } = 100;
        public TimeSpan FlushInterval { get; set; } = TimeSpan.FromMilliseconds(100);

        public WatchLoggerProvider(
            ISwitchSource switchSource,
            IEventSinkProvider sink,
            IObjectSerializer objectSerializer = null)
        {
            SwitchSource = switchSource ?? throw new ArgumentNullException(nameof(switchSource));
            Sink = sink ?? throw new ArgumentNullException(nameof(sink));
            ObjectSerializer = objectSerializer ?? NewtonsoftObjectSerializer.Instance;

            SwitchSource.Start();
            Sink.Start();

            _flushTimer = new Timer(FlushCallback, null, FlushInterval, FlushInterval);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new WatchLogger(this, name));
        }

        internal void Accept(LogEvent logEvent)
        {
            if (_disposed)
                return;

            _queue.Enqueue(logEvent);
        }

        private void FlushCallback(object state)
        {
            if (Interlocked.CompareExchange(ref _flushing, 1, 0) != 0)
                return;

            try
            {
                FlushQueue();
            }
            finally
            {
                Interlocked.Exchange(ref _flushing, 0);
            }
        }

        private void FlushQueue()
        {
            while (!_queue.IsEmpty)
            {
                var batch = new List<LogEvent>(BatchSize);

                while (batch.Count < BatchSize && _queue.TryDequeue(out var logEvent))
                {
                    batch.Add(logEvent);
                }

                if (batch.Count > 0)
                {
                    var eventBatch = new LogEventBatch
                    {
                        Domain = Domain,
                        Events = batch
                    };

                    try
                    {
                        Sink.Accept(eventBatch, CancellationToken.None).GetAwaiter().GetResult();
                    }
                    catch
                    {
                        // Sink failures are handled by the sink itself
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _flushTimer.Dispose();

            // Final flush
            FlushQueue();

            try
            {
                Sink.Stop();
            }
            catch
            {
                // Ignore
            }

            try
            {
                SwitchSource.Stop();
            }
            catch
            {
                // Ignore
            }
        }
    }
}
