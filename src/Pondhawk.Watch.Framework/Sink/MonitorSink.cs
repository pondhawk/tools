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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pondhawk.Watch.Framework.Sink
{
    public class MonitorSink : IEventSinkProvider
    {
        private readonly ConcurrentBag<LogEvent> _events = new ConcurrentBag<LogEvent>();

        public bool Accumulate { get; set; }

        public void Start() { }
        public void Stop() { }

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

        public async Task<IReadOnlyList<LogEvent>> WaitForEventsAsync(int count = 1, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(2));
            while (_events.Count < count && DateTime.UtcNow < deadline)
                await Task.Delay(25);
            return _events.ToList();
        }

        public IReadOnlyList<LogEvent> GetEvents() => _events.ToList();

        public int Count => _events.Count;

        public void Clear()
        {
            while (_events.TryTake(out _)) { }
        }
    }
}
