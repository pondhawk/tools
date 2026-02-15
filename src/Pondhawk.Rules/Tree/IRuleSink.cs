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
/// Accepts rules during rule set construction, indexing them by their fact types.
/// </summary>
public interface IRuleSink
{
    /// <summary>Adds a single rule indexed by the specified fact type.</summary>
    /// <param name="factType">The fact type this rule operates on.</param>
    /// <param name="rules">The rule to add.</param>
    void Add(Type factType, IRule rules);

    /// <summary>Adds a collection of rules indexed by the specified fact types.</summary>
    /// <param name="factTypes">The fact types these rules operate on.</param>
    /// <param name="rules">The rules to add.</param>
    void Add(Type[] factTypes, IEnumerable<IRule> rules);
}


