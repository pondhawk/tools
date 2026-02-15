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

    protected FluentException(string message) : base(message)
    {
    }

    protected FluentException(string message, Exception inner) : base(message, inner)
    {
    }


    public TDescendant WithKind(ErrorKind kind)
    {
        Kind = kind;
        return (TDescendant)this;
    }

    public TDescendant WithErrorCode(string code)
    {
        Guard.IsNotNull(code);
        ErrorCode = code;
        return (TDescendant)this;
    }

    public TDescendant WithExplanation(string explanation)
    {
        Guard.IsNotNull(explanation);
        Explanation = explanation;
        return (TDescendant)this;

    }

    public TDescendant WithCorrelationId(string correlationId)
    {
        Guard.IsNotNull(correlationId);
        CorrelationId = correlationId;
        return (TDescendant)this;

    }

    public TDescendant WithDetail(EventDetail detail)
    {

        Guard.IsNotNull(detail);

        Details.Add(detail);
        return (TDescendant)this;

    }

    public TDescendant WithDetails(IEnumerable<EventDetail> details)
    {
        Guard.IsNotNull(details);

        foreach (var d in details)
            Details.Add(d);

        return (TDescendant)this;

    }

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
