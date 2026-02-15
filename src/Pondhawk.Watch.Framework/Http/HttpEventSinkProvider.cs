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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Newtonsoft.Json;

namespace Pondhawk.Watch.Framework.Http
{
    public class HttpEventSinkProvider : IEventSinkProvider
    {
        private readonly HttpClient _client;
        private readonly string _domain;

        private int _consecutiveFailures;
        private DateTime _circuitOpenUntil = DateTime.MinValue;
        private readonly object _circuitLock = new object();

        private readonly ConcurrentQueue<LogEvent> _criticalBuffer = new ConcurrentQueue<LogEvent>();

        public int FailureThreshold { get; set; } = 3;
        public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(2);
        public int MaxCriticalBufferSize { get; set; } = 1000;

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

        public int CriticalBufferCount => _criticalBuffer.Count;

        public long DroppedEventCount => Interlocked.Read(ref _droppedEventCount);
        private long _droppedEventCount;

        private static readonly JsonSerializerSettings WireSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        public HttpEventSinkProvider(HttpClient client, string domain)
        {
            Guard.IsNotNull(client);
            Guard.IsNotNull(domain);

            _client = client;
            _domain = domain;
        }

        public void Start() { }
        public void Stop() { }

        public async Task Accept(LogEventBatch batch, CancellationToken cancellationToken = default)
        {
            if (IsCircuitOpen)
            {
                BufferCriticalEvents(batch);
                return;
            }

            try
            {
                FlushCriticalBuffer(batch);
                await SendBatchAsync(batch, cancellationToken).ConfigureAwait(false);
                OnSuccess();
            }
            catch
            {
                OnFailure(batch);
            }
        }

        private async Task SendBatchAsync(LogEventBatch batch, CancellationToken ct)
        {
            var json = JsonConvert.SerializeObject(batch, WireSettings);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = await _client.PostAsync("api/sink", content, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
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
                    var backoffFactor = Math.Pow(2, _consecutiveFailures - FailureThreshold);
                    var delay = TimeSpan.FromTicks((long)(BaseRetryDelay.Ticks * backoffFactor));

                    if (delay > MaxRetryDelay)
                        delay = MaxRetryDelay;

                    _circuitOpenUntil = DateTime.UtcNow.Add(delay);
                }
            }

            BufferCriticalEvents(batch);
        }

        private void BufferCriticalEvents(LogEventBatch batch)
        {
            foreach (var e in batch.Events)
            {
                if (e.Level >= (int)Level.Warning)
                {
                    LogEvent dummy;
                    while (_criticalBuffer.Count >= MaxCriticalBufferSize)
                    {
                        if (_criticalBuffer.TryDequeue(out dummy))
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
            LogEvent e;
            while (_criticalBuffer.TryDequeue(out e))
            {
                batch.Events.Insert(0, e);
            }
        }
    }
}
