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

using Pondhawk.Rules.Builder;

namespace Pondhawk.Rules.Tree;

/// <summary>
/// An indexed collection of rules organized by fact types for efficient lookup during evaluation.
/// </summary>
public interface IRuleBase
{
    /// <summary>Gets the maximum fact-type arity (number of fact types per rule) in this rule base.</summary>
    int MaxAxisCount { get; }

    /// <summary>Returns <c>true</c> if any rules exist for the given fact types.</summary>
    /// <param name="factTypes">The fact types to check.</param>
    /// <returns><c>true</c> if matching rules exist; otherwise <c>false</c>.</returns>
    bool HasRules(Type[] factTypes);

    /// <summary>Returns <c>true</c> if any rules exist for the given fact types within the specified namespaces.</summary>
    /// <param name="factTypes">The fact types to check.</param>
    /// <param name="namespaces">The namespace filters to apply.</param>
    /// <returns><c>true</c> if matching rules exist; otherwise <c>false</c>.</returns>
    bool HasRules(Type[] factTypes, IEnumerable<string> namespaces);

    /// <summary>Finds all rules that match the given fact types.</summary>
    /// <param name="factTypes">The fact types to match against.</param>
    /// <returns>A set of matching rules.</returns>
    ISet<IRule> FindRules(Type[] factTypes);

    /// <summary>Finds all rules that match the given fact types within the specified namespaces.</summary>
    /// <param name="factTypes">The fact types to match against.</param>
    /// <param name="namespaces">The namespace filters to apply.</param>
    /// <returns>A set of matching rules.</returns>
    ISet<IRule> FindRules(Type[] factTypes, IEnumerable<string> namespaces);
}

