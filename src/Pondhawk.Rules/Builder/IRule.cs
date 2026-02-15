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

using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Rules.Builder;

/// <summary>
/// Represents a single rule with conditions and a consequence. Rules are matched against facts during evaluation.
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not conflict with reserved keywords", Justification = "Namespace is the established domain term for rule grouping and cannot be renamed without breaking the public API")]
public interface IRule
{
    /// <summary>Gets the namespace (set name) that this rule belongs to.</summary>
    string Namespace { get; }

    /// <summary>Gets the fully qualified name of this rule.</summary>
    string Name { get; }

    /// <summary>Gets the salience (priority) of this rule; higher values evaluate first.</summary>
    int Salience { get; }

    /// <summary>Gets a value indicating whether this rule fires at most once per evaluation session.</summary>
    bool OnlyFiresOnce { get; }

    /// <summary>Gets the mutex group name; only one rule in a mutex group fires per tuple.</summary>
    string Mutex { get; }

    /// <summary>Gets the earliest date/time at which this rule becomes active.</summary>
    DateTime Inception { get; }

    /// <summary>Gets the latest date/time at which this rule remains active.</summary>
    DateTime Expiration { get; }

    /// <summary>Evaluates this rule's conditions against the given facts.</summary>
    /// <param name="fact">The fact tuple to evaluate against.</param>
    /// <returns>This rule instance if conditions are met; otherwise <c>null</c>.</returns>
    IRule EvaluateRule(object[] fact);

    /// <summary>Fires this rule's consequence against the given facts.</summary>
    /// <param name="fact">The fact tuple to fire against.</param>
    void FireRule(object[] fact);
}

