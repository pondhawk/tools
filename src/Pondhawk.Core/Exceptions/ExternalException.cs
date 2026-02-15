/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

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

namespace Pondhawk.Exceptions
{

    /// <summary>
    /// Base exception for external/application-facing errors with error kind, code, explanation, and event details.
    /// </summary>
    public abstract class ExternalException : Exception
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalException"/> class with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        protected ExternalException(string message) : base(message)
        {

            ErrorCode = GetType().Name.Replace("Exception", "");
            Explanation = message;

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalException"/> class with the specified message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception that caused this exception.</param>
        protected ExternalException(string message, Exception inner) : base(message, inner)
        {

            ErrorCode = GetType().Name.Replace("Exception", "");
            Explanation = message;

            if (inner is not InternalException intra)
                return;

            InnerExplanation = intra.Explanation;

            foreach (var detail in intra.Details)
                Details.Add(detail);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalException"/> class from an <see cref="IExceptionInfo"/>.
        /// </summary>
        /// <param name="info">The exception info to populate this exception from.</param>
        protected ExternalException(IExceptionInfo info) : base(info.Explanation)
        {

            Kind = info.Kind;
            ErrorCode = GetType().Name.Replace("Exception", "");
            Explanation = info.Explanation;
            InnerExplanation = info.Explanation;

            Details = new List<EventDetail>(info.Details);

        }



        /// <summary>
        /// Gets the classification of this error.
        /// </summary>
        public ErrorKind Kind { get; protected set; } = ErrorKind.System;

        /// <summary>
        /// Gets the error code, derived from the exception type name by default.
        /// </summary>
        public string ErrorCode { get; protected set; }

        /// <summary>
        /// Gets the human-readable explanation of the error.
        /// </summary>
        public string Explanation { get; protected set; }

        /// <summary>
        /// Gets the explanation from the inner exception, if available.
        /// </summary>
        public string InnerExplanation { get; protected set; } = "";

        /// <summary>
        /// Gets the correlation identifier for tracing this error across systems.
        /// </summary>
        public string CorrelationId { get; protected set; } = "";


        /// <summary>
        /// Gets the list of <see cref="EventDetail"/> instances associated with this exception.
        /// </summary>
        public IList<EventDetail> Details { get; protected set; } = new List<EventDetail>();

    }

}
