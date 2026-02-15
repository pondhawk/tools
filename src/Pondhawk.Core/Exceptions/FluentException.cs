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

using CommunityToolkit.Diagnostics;

namespace Pondhawk.Exceptions;

/// <summary>
/// Base exception with a fluent builder API for setting kind, error code, explanation, and details.
/// </summary>
public abstract class FluentException<TDescendant> : ExternalException where TDescendant : FluentException<TDescendant>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentException{TDescendant}"/> class with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    protected FluentException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentException{TDescendant}"/> class with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception that caused this exception.</param>
    protected FluentException(string message, Exception inner) : base(message, inner)
    {
    }


    /// <summary>
    /// Sets the <see cref="ExternalException.Kind"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="kind">The error kind to set.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithKind(ErrorKind kind)
    {
        Kind = kind;
        return (TDescendant)this;
    }

    /// <summary>
    /// Sets the <see cref="ExternalException.ErrorCode"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="code">The error code to set.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithErrorCode(string code)
    {
        Guard.IsNotNull(code);
        ErrorCode = code;
        return (TDescendant)this;
    }

    /// <summary>
    /// Sets the <see cref="ExternalException.Explanation"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="explanation">The explanation text to set.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithExplanation(string explanation)
    {
        Guard.IsNotNull(explanation);
        Explanation = explanation;
        return (TDescendant)this;

    }

    /// <summary>
    /// Sets the <see cref="ExternalException.CorrelationId"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="correlationId">The correlation identifier to set.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithCorrelationId(string correlationId)
    {
        Guard.IsNotNull(correlationId);
        CorrelationId = correlationId;
        return (TDescendant)this;

    }

    /// <summary>
    /// Adds a single <see cref="EventDetail"/> to this exception and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="detail">The event detail to add.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithDetail(EventDetail detail)
    {

        Guard.IsNotNull(detail);

        Details.Add(detail);
        return (TDescendant)this;

    }

    /// <summary>
    /// Adds multiple <see cref="EventDetail"/> instances to this exception and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="details">The event details to add.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithDetails(IEnumerable<EventDetail> details)
    {
        Guard.IsNotNull(details);

        foreach (var d in details)
            Details.Add(d);

        return (TDescendant)this;

    }

    /// <summary>
    /// Populates this exception from an <see cref="IExceptionInfo"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="info">The exception info to copy kind, error code, explanation, and details from.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant With(IExceptionInfo info)
    {

        Guard.IsNotNull(info);

        WithKind(info.Kind);
        WithErrorCode(info.ErrorCode);
        WithExplanation(info.Explanation);
        WithDetails(info.Details);


        return (TDescendant)this;

    }



}
