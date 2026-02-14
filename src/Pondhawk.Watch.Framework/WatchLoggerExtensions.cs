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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Pondhawk.Watch.Framework.Serializers;
using Pondhawk.Watch.Framework.StateTypes;

namespace Pondhawk.Watch.Framework
{
    public static class WatchLoggerExtensions
    {
        #region Method Tracing

        private static readonly ConcurrentQueue<MethodScope> ScopePool = new ConcurrentQueue<MethodScope>();
        private const int MaxPoolSize = 256;

        public static IDisposable EnterMethod(
            this ILogger logger,
            [CallerMemberName] string method = "",
            [CallerFilePath] string file = "")
        {
            if (!logger.IsEnabled(LogLevel.Trace))
                return NullScope.Instance;

            var entry = new MethodEntry(method, file);

            logger.Log(
                LogLevel.Trace,
                WatchEventIds.MethodEntry,
                entry,
                null,
                (s, _) => s.ToString());

            MethodScope scope;
            if (!ScopePool.TryDequeue(out scope))
                scope = new MethodScope();

            scope.Initialize(logger, entry.ClassName, method);
            return scope;
        }

        private sealed class MethodScope : IDisposable
        {
            private ILogger _logger;
            private string _className;
            private string _method;
            private long _startTimestamp;
            private bool _disposed;

            public void Initialize(ILogger logger, string className, string method)
            {
                _logger = logger;
                _className = className;
                _method = method;
                _startTimestamp = Stopwatch.GetTimestamp();
                _disposed = false;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                var elapsed = TimeSpan.FromTicks(
                    (Stopwatch.GetTimestamp() - _startTimestamp) * TimeSpan.TicksPerSecond / Stopwatch.Frequency);

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    var exit = new MethodExit(_method, _className, elapsed);

                    _logger.Log(
                        LogLevel.Trace,
                        WatchEventIds.MethodExit,
                        exit,
                        null,
                        (s, _) => s.ToString());
                }

                _logger = null;
                _className = null;
                _method = null;

                if (ScopePool.Count < MaxPoolSize)
                    ScopePool.Enqueue(this);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();
            public void Dispose() { }
        }

        #endregion

        #region Object Serialization

        public static void LogObject<T>(
            this ILogger logger,
            T value,
            LogLevel level = LogLevel.Debug)
        {
            if (!logger.IsEnabled(level))
                return;

            var (_, json) = NewtonsoftObjectSerializer.Instance.Serialize(value);
            var payload = new TypedPayload(PayloadType.Json, json);
            var typeName = typeof(T).Name;

            logger.Log(
                level,
                WatchEventIds.Object,
                payload,
                null,
                (__, ___) => typeName);
        }

        public static void LogObject<T>(
            this ILogger logger,
            string title,
            T value,
            LogLevel level = LogLevel.Debug)
        {
            if (!logger.IsEnabled(level))
                return;

            var (_, json) = NewtonsoftObjectSerializer.Instance.Serialize(value);
            var payload = new TypedPayload(PayloadType.Json, json);

            logger.Log(
                level,
                WatchEventIds.Object,
                payload,
                null,
                (__, ___) => title);
        }

        #endregion

        #region Typed Payloads

        public static void LogJson(
            this ILogger logger,
            string title,
            string json,
            LogLevel level = LogLevel.Debug)
        {
            if (!logger.IsEnabled(level))
                return;

            var payload = new TypedPayload(PayloadType.Json, json);

            logger.Log(
                level,
                WatchEventIds.Payload,
                payload,
                null,
                (_, __) => title);
        }

        public static void LogSql(
            this ILogger logger,
            string title,
            string sql,
            LogLevel level = LogLevel.Debug)
        {
            if (!logger.IsEnabled(level))
                return;

            var payload = new TypedPayload(PayloadType.Sql, sql);

            logger.Log(
                level,
                WatchEventIds.Payload,
                payload,
                null,
                (_, __) => title);
        }

        public static void LogXml(
            this ILogger logger,
            string title,
            string xml,
            LogLevel level = LogLevel.Debug)
        {
            if (!logger.IsEnabled(level))
                return;

            var payload = new TypedPayload(PayloadType.Xml, xml);

            logger.Log(
                level,
                WatchEventIds.Payload,
                payload,
                null,
                (_, __) => title);
        }

        public static void LogYaml(
            this ILogger logger,
            string title,
            string yaml,
            LogLevel level = LogLevel.Debug)
        {
            if (!logger.IsEnabled(level))
                return;

            var payload = new TypedPayload(PayloadType.Yaml, yaml);

            logger.Log(
                level,
                WatchEventIds.Payload,
                payload,
                null,
                (_, __) => title);
        }

        public static void LogText(
            this ILogger logger,
            string title,
            string text,
            LogLevel level = LogLevel.Debug)
        {
            if (!logger.IsEnabled(level))
                return;

            var payload = new TypedPayload(PayloadType.Text, text);

            logger.Log(
                level,
                WatchEventIds.Payload,
                payload,
                null,
                (_, __) => title);
        }

        #endregion
    }
}
