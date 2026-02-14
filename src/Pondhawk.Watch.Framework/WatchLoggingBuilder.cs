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
using System.Net.Http;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Pondhawk.Watch.Framework.Http;
using Pondhawk.Watch.Framework.Sink;
using Pondhawk.Watch.Framework.Switching;

namespace Pondhawk.Watch.Framework
{
    public class WatchLoggingBuilder
    {
        private readonly WatchLoggingOptions _options = new WatchLoggingOptions();
        private ISwitchSource _switchSource;
        private IEventSinkProvider _sink;
        private bool _useQuiet;
        private HttpClient _httpClient;

        public static WatchLoggingBuilder Create() => new WatchLoggingBuilder();

        private WatchLoggingBuilder() { }

        public WatchLoggingBuilder WithDomain(string domain)
        {
            _options.Domain = domain;
            return this;
        }

        public WatchLoggingBuilder WithBatchSize(int size)
        {
            _options.BatchSize = size;
            return this;
        }

        public WatchLoggingBuilder WithFlushInterval(TimeSpan interval)
        {
            _options.FlushIntervalMs = (int)interval.TotalMilliseconds;
            return this;
        }

        public WatchLoggingBuilder UseQuiet()
        {
            _useQuiet = true;
            return this;
        }

        public WatchLoggingBuilder UseConsole()
        {
            _sink = new ConsoleEventSink();
            return this;
        }

        public WatchLoggingBuilder UseSink(IEventSinkProvider sink)
        {
            Guard.IsNotNull(sink);
            _sink = sink;
            return this;
        }

        public WatchLoggingBuilder UseHttpSink(string baseUrl, string domain)
        {
            if (_httpClient == null)
                _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sink = new HttpEventSinkProvider(_httpClient, domain);
            _options.Domain = domain;
            return this;
        }

        public WatchLoggingBuilder UseHttpSwitchSource(string baseUrl, string domain, TimeSpan? pollInterval = null)
        {
            if (_httpClient == null)
                _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _switchSource = new HttpSwitchSource(_httpClient, domain, pollInterval);
            return this;
        }

        public WatchLoggingBuilder UseLocalSwitchSource(Action<SwitchSource> configure = null)
        {
            var source = new SwitchSource();
            configure?.Invoke(source);
            _switchSource = source;
            return this;
        }

        public WatchLoggingBuilder WhenNotMatched(Level level, Color? color = null)
        {
            var source = _switchSource as SwitchSource ?? new SwitchSource();
            source.WhenNotMatched(level, color ?? Color.LightGray);
            _switchSource = source;
            return this;
        }

        public WatchLoggingBuilder WhenMatched(string pattern, Level level, Color color)
        {
            var source = _switchSource as SwitchSource ?? new SwitchSource();
            source.WhenMatched(pattern, level, color);
            _switchSource = source;
            return this;
        }

        public ILoggerProvider Build()
        {
            if (_useQuiet)
            {
                return QuietWatchLoggerProvider.Instance;
            }

            if (_switchSource == null)
                _switchSource = new SwitchSource();
            if (_sink == null)
                _sink = new ConsoleEventSink();

            return new WatchLoggerProvider(_switchSource, _sink)
            {
                Domain = _options.Domain,
                BatchSize = _options.BatchSize,
                FlushInterval = _options.FlushInterval
            };
        }
    }
}
