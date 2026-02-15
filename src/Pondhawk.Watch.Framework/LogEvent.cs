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
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Pondhawk.Watch.Framework
{
    public class LogEvent
    {
        public string Category { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Color { get; set; }
        public int Nesting { get; set; }
        public DateTime Occurred { get; set; }
        public int Type { get; set; }
        public string Payload { get; set; }
        public string ErrorType { get; set; }

        [JsonIgnore]
        [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Domain-specific name for the logged object")]
        public object Object { get; set; }

        [JsonIgnore]
        public Exception Error { get; set; }

        [JsonIgnore]
        public object ErrorContext { get; set; }
    }
}
