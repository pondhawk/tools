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
using System.Text;
using System.Text.RegularExpressions;

namespace Pondhawk.Watch.Framework.Utilities
{
    public static class MessageTemplate
    {
        private static readonly Regex PlaceholderPattern = new Regex(
            @"\{(?<name>[^{}:]+)(?::[^{}]*)?\}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture,
            TimeSpan.FromSeconds(1));

        public static string[] GetPlaceholderNames(string template)
        {
            if (string.IsNullOrEmpty(template))
                return Array.Empty<string>();

            var matches = PlaceholderPattern.Matches(template);
            var names = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                names[i] = matches[i].Groups["name"].Value;
            }

            return names;
        }

        public static string Format(string template, params object[] values)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            if (values == null || values.Length == 0)
                return template;

            var result = new StringBuilder(template);
            var matches = PlaceholderPattern.Matches(template);

            for (int i = Math.Min(matches.Count, values.Length) - 1; i >= 0; i--)
            {
                var match = matches[i];
                var value = values[i]?.ToString() ?? "null";
                result.Remove(match.Index, match.Length);
                result.Insert(match.Index, value);
            }

            return result.ToString();
        }

        public static IDictionary<string, object> BuildDictionary(string[] names, object[] values)
        {
            var dict = new Dictionary<string, object>(StringComparer.Ordinal);

            for (int i = 0; i < Math.Min(names.Length, values.Length); i++)
            {
                dict[names[i]] = values[i];
            }

            return dict;
        }
    }
}
