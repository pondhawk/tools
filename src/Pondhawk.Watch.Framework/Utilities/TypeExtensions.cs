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
using System.Linq;

namespace Pondhawk.Watch.Framework.Utilities
{
    internal static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> ConciseNameCache = new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<Type, string> ConciseFullNameCache = new ConcurrentDictionary<Type, string>();

        public static string GetConciseName(this Type type)
        {
            return ConciseNameCache.GetOrAdd(type, ComputeConciseName);
        }

        public static string GetConciseFullName(this Type type)
        {
            return ConciseFullNameCache.GetOrAdd(type, ComputeConciseFullName);
        }

        private static string ComputeConciseName(Type type)
        {
            var conciseName = type.Name;
            if (!type.IsGenericType)
                return conciseName;

            var iBacktick = conciseName.IndexOf('`');
            if (iBacktick > 0)
                conciseName = conciseName.Substring(0, iBacktick);

            var genericParameters = type.GetGenericArguments().Select(x => x.GetConciseName());
            conciseName += "<" + string.Join(", ", genericParameters) + ">";

            return conciseName;
        }

        private static string ComputeConciseFullName(Type type)
        {
            var conciseName = type.FullName;
            if (string.IsNullOrWhiteSpace(conciseName))
                return string.Empty;

            if (!type.IsGenericType)
                return conciseName;

            var iBacktick = conciseName.IndexOf('`');
            if (iBacktick > 0)
                conciseName = conciseName.Substring(0, iBacktick);

            var genericParameters = type.GetGenericArguments().Select(x => x.GetConciseName());
            conciseName += "<" + string.Join(", ", genericParameters) + ">";

            return conciseName;
        }
    }
}
