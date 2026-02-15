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
using System.Collections.Immutable;
using System.Threading;

namespace Pondhawk.Watch.Framework
{
    internal static class LoggingScopeManager
    {
        private static readonly AsyncLocal<ImmutableStack<object>> Scopes = new AsyncLocal<ImmutableStack<object>>();

        public static bool HasScopes => !(Scopes.Value?.IsEmpty ?? true);

        public static IDisposable Push(object state)
        {
            Scopes.Value = (Scopes.Value ?? ImmutableStack<object>.Empty).Push(state);
            return new ScopeDisposable();
        }

        public static IEnumerable<object> GetScopes() =>
            Scopes.Value ?? (IEnumerable<object>)ImmutableStack<object>.Empty;

        internal static ImmutableStack<object> Current =>
            Scopes.Value ?? ImmutableStack<object>.Empty;

        private sealed class ScopeDisposable : IDisposable
        {
            private int _disposed;

            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                    return;

                var current = Scopes.Value;
                if (current is null || current.IsEmpty)
                    return;

                Scopes.Value = current.Pop();
            }
        }
    }
}
