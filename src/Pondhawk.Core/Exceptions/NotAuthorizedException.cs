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

namespace Pondhawk.Exceptions;

/// <summary>
/// Exception thrown when the caller is not authorized to perform an operation. Fixes
/// <see cref="ErrorKind.NotAuthorized"/>. This is sugar over <c>WithKind</c>; mapping is always
/// keyed on <see cref="ExternalException.Kind"/>, never on the concrete type.
/// </summary>
public sealed class NotAuthorizedException : FluentException<NotAuthorizedException>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotAuthorizedException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message describing the authorization failure.</param>
    public NotAuthorizedException(string message)
        : base(message)
    {
        WithKind(ErrorKind.NotAuthorized);
        WithErrorCode("NotAuthorized");
    }
}
