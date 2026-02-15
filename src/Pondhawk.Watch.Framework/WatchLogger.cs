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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondhawk.Watch.Framework.StateTypes;
using Pondhawk.Watch.Framework.Utilities;

namespace Pondhawk.Watch.Framework
{
    public sealed class WatchLogger : ILogger
    {
        private readonly WatchLoggerProvider _provider;
        private readonly string _category;

        private ISwitch _cachedSwitch;
        private long _cachedVersion = -1;

        internal WatchLogger(WatchLoggerProvider provider, string category)
        {
            Guard.IsNotNull(provider);
            Guard.IsNotNull(category);

            _provider = provider;
            _category = category;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
                return false;

            var sw = GetSwitch();
            if (sw.Level == Level.Quiet)
                return false;

            return MapLevel(logLevel) >= sw.Level;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var sw = GetSwitch();
            var title = formatter(state, exception);

            var le = new LogEvent
            {
                Category = _category,
                Level = (int)MapLevel(logLevel),
                Color = sw.Color.ToArgb(),
                Tag = sw.Tag,
                Title = title,
                CorrelationId = GetCorrelationId(),
                Occurred = DateTime.UtcNow
            };

            if (eventId == WatchEventIds.MethodEntry)
            {
                le.Nesting = 1;
            }
            else if (eventId == WatchEventIds.MethodExit)
            {
                le.Nesting = -1;
            }

            if (state is TypedPayload typed)
            {
                le.Type = (int)typed.Type;
                le.Payload = typed.Content;
            }
            else if (exception != null)
            {
                le.ErrorType = exception.GetType().FullName ?? exception.GetType().Name;
                le.Error = exception;
            }
            else
            {
                var payload = BuildPayload(state);
                if (payload != null)
                {
                    le.Type = (int)PayloadType.Json;
                    le.Payload = payload;
                }
            }

            _provider.Accept(le);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return LoggingScopeManager.Push(state);
        }

        private ISwitch GetSwitch()
        {
            var currentVersion = _provider.SwitchSource.Version;
            var cachedVersion = Volatile.Read(ref _cachedVersion);

            if (_cachedSwitch == null || cachedVersion != currentVersion)
            {
                var newSwitch = _provider.SwitchSource.Lookup(_category);
                _cachedSwitch = newSwitch;
                Volatile.Write(ref _cachedVersion, currentVersion);
            }

            return _cachedSwitch;
        }

        private static Level MapLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace: return Level.Trace;
                case LogLevel.Debug: return Level.Debug;
                case LogLevel.Information: return Level.Info;
                case LogLevel.Warning: return Level.Warning;
                case LogLevel.Error: return Level.Error;
                case LogLevel.Critical: return Level.Error;
                default: return Level.Debug;
            }
        }

        private static string GetCorrelationId()
        {
            var activity = Activity.Current;
            if (activity != null)
            {
                var correlation = activity.GetBaggageItem(LoggingCorrelation.BaggageKey);
                if (!string.IsNullOrEmpty(correlation))
                    return correlation;

                var newId = Ulid.NewUlid();
                activity.AddBaggage(LoggingCorrelation.BaggageKey, newId);
                return newId;
            }

            return Ulid.NewUlid();
        }

        private static string BuildPayload<TState>(TState state)
        {
            if (state is IReadOnlyList<KeyValuePair<string, object>> kvps)
            {
                var dict = new Dictionary<string, object>(StringComparer.Ordinal);

                foreach (var kvp in kvps)
                {
                    if (string.Equals(kvp.Key, "{OriginalFormat}", StringComparison.Ordinal))
                        continue;

                    dict[kvp.Key] = kvp.Value;
                }

                if (LoggingScopeManager.HasScopes)
                {
                    var scopes = new List<object>();
                    foreach (var scope in LoggingScopeManager.GetScopes())
                    {
                        scopes.Add(scope);
                    }

                    if (scopes.Count > 0)
                    {
                        dict["Scopes"] = scopes;
                    }
                }

                if (dict.Count > 0)
                {
                    try
                    {
                        return JsonConvert.SerializeObject(dict);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}
