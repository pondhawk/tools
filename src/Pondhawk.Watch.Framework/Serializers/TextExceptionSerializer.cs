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
using System.Text;

namespace Pondhawk.Watch.Framework.Serializers
{
    public class TextExceptionSerializer : IExceptionSerializer
    {
        public static readonly TextExceptionSerializer Instance = new TextExceptionSerializer();

        public (PayloadType Type, string Payload) Serialize(Exception exception, object context)
        {
            var sb = new StringBuilder();
            SerializeException(sb, exception, 0);
            return (PayloadType.Text, sb.ToString());
        }

        private static void SerializeException(StringBuilder sb, Exception ex, int depth)
        {
            var indent = new string(' ', depth * 2);

            sb.AppendLine($"{indent}Exception Type: {ex.GetType().FullName}");
            sb.AppendLine($"{indent}Message: {ex.Message}");

            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                sb.AppendLine($"{indent}Stack Trace:");
                var lines = ex.StackTrace.Split('\n');
                foreach (var line in lines)
                {
                    sb.AppendLine($"{indent}  {line.Trim()}");
                }
            }

            if (ex is AggregateException agg)
            {
                sb.AppendLine($"{indent}Inner Exceptions ({agg.InnerExceptions.Count}):");
                for (var i = 0; i < agg.InnerExceptions.Count; i++)
                {
                    sb.AppendLine($"{indent}  [{i}]:");
                    SerializeException(sb, agg.InnerExceptions[i], depth + 2);
                }
            }
            else if (ex.InnerException != null)
            {
                sb.AppendLine($"{indent}Inner Exception:");
                SerializeException(sb, ex.InnerException, depth + 1);
            }
        }
    }
}
