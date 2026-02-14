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
using System.Diagnostics;
using Pondhawk.Watch.Framework.Utilities;

namespace Pondhawk.Watch.Framework
{
    public static class LoggingCorrelation
    {
        public const string BaggageKey = "watch.correlation";

        public static IDisposable Begin()
        {
            return Begin(Ulid.NewUlid());
        }

        public static IDisposable Begin(string correlationId)
        {
            var activity = new Activity("LoggingCorrelation");
            activity.AddBaggage(BaggageKey, correlationId);
            activity.Start();
            return new CorrelationScope(activity);
        }

        public static void Set(string correlationId = null)
        {
            var id = correlationId ?? Ulid.NewUlid();
            Activity.Current?.AddBaggage(BaggageKey, id);
        }

        public static string Current => Activity.Current?.GetBaggageItem(BaggageKey);

        private sealed class CorrelationScope : IDisposable
        {
            private readonly Activity _activity;
            private bool _disposed;

            public CorrelationScope(Activity activity)
            {
                _activity = activity;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _activity.Stop();
                _activity.Dispose();
            }
        }
    }
}
