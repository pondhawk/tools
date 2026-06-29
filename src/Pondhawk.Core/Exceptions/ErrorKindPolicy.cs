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
/// Canonical, transport-agnostic retry policy for <see cref="ErrorKind"/> values so that
/// every queue consumer routes <c>retry vs. dead-letter</c> identically by default.
/// </summary>
/// <remarks>
/// Consumers may override this policy, but it is the shared default. Note that the HTTP
/// status-code mapping is deliberately NOT defined here — that belongs to the ASP.NET layer.
/// </remarks>
public static class ErrorKindPolicy
{
    /// <summary>
    /// Indicates whether an error of the given kind is transient and therefore worth
    /// retrying / requeuing. Everything else is treated as permanent (dead-letter).
    /// </summary>
    /// <param name="kind">The error kind to classify.</param>
    /// <returns><see langword="true"/> if the kind is transient; otherwise <see langword="false"/>.</returns>
    public static bool IsTransient(this ErrorKind kind) =>
        kind is ErrorKind.System or ErrorKind.Remote or ErrorKind.Concurrency;
}
