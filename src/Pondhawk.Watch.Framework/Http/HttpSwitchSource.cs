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
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CommunityToolkit.Diagnostics;
using Newtonsoft.Json;
using Pondhawk.Watch.Framework.Switching;

namespace Pondhawk.Watch.Framework.Http
{
    public class HttpSwitchSource : SwitchSource, IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _domain;
        private readonly TimeSpan _pollInterval;
        private Timer _pollTimer;
        private readonly object _startLock = new object();
        private bool _started;
        private bool _disposed;

        public bool PollingEnabled { get; set; } = true;

        public HttpSwitchSource(HttpClient client, string domain, TimeSpan? pollInterval = null)
        {
            Guard.IsNotNull(client);
            Guard.IsNotNull(domain);

            _client = client;
            _domain = domain;
            _pollInterval = pollInterval ?? TimeSpan.FromSeconds(30);
        }

        public override void Start()
        {
            lock (_startLock)
            {
                if (_started)
                    return;
                _started = true;
            }

            FetchSwitches();

            if (PollingEnabled)
            {
                _pollTimer = new Timer(PollCallback, null, _pollInterval, _pollInterval);
            }
        }

        public override void Stop()
        {
            _pollTimer?.Dispose();
            _pollTimer = null;
        }

        private void PollCallback(object state)
        {
            try
            {
                FetchSwitches();
            }
            catch
            {
                // Continue polling even on failure
            }
        }

        private void FetchSwitches()
        {
            try
            {
                var url = $"api/switches?domain={Uri.EscapeDataString(_domain)}";
                var responseBody = _client.GetStringAsync(url).GetAwaiter().GetResult();
                var response = JsonConvert.DeserializeObject<SwitchesResponse>(responseBody);

                if (response?.Switches != null)
                {
                    var defs = response.Switches.Select(s => new SwitchDef
                    {
                        Pattern = s.Pattern,
                        Tag = s.Tag,
                        Level = (Level)s.Level,
                        Color = Color.FromArgb(s.Color)
                    }).ToList();

                    Update(defs);
                }
            }
            catch
            {
                // Silently ignore failures - we'll try again next poll
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
        }
    }
}
