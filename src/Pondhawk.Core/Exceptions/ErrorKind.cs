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
/// Classifies errors by kind (e.g. NotFound, Predicate, System, Functional).
/// </summary>
public enum ErrorKind
{
    /// <summary>The error kind is unknown or unspecified.</summary>
    Unknown,
    /// <summary>A predicate or validation constraint was not satisfied.</summary>
    Predicate,
    /// <summary>The request was malformed or invalid.</summary>
    BadRequest,
    /// <summary>A business-logic or functional error occurred.</summary>
    Functional,
    /// <summary>The requested operation is not implemented.</summary>
    NotImplemented,
    /// <summary>The requested resource was not found.</summary>
    NotFound,
    /// <summary>A concurrency conflict occurred.</summary>
    Concurrency,
    /// <summary>A system-level or infrastructure error occurred.</summary>
    System,
    /// <summary>No error; the operation succeeded.</summary>
    None,
    /// <summary>Authentication is required to perform the operation.</summary>
    AuthenticationRequired,
    /// <summary>The caller is not authorized to perform the operation.</summary>
    NotAuthorized,
    /// <summary>A conflict with the current state of the resource was detected.</summary>
    Conflict,
    /// <summary>An error occurred in a remote service or dependency.</summary>
    Remote
}
