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
using Pondhawk.Rules.Builder;

namespace Pondhawk.Rules.Listeners;

/// <summary>
/// Observer interface for tracing rule evaluation. Receives callbacks for evaluation lifecycle events.
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not conflict with reserved keywords", Justification = "Parameter name 'template' is the established domain term for message templates and cannot be renamed without breaking the public API")]
public interface IEvaluationListener
{
    /// <summary>
    /// Called when a rule evaluation session begins.
    /// </summary>
    void BeginEvaluation();

    /// <summary>
    /// Called when evaluation of a specific fact tuple begins.
    /// </summary>
    /// <param name="facts">The array of fact objects being evaluated.</param>
    void BeginTupleEvaluation(object[] facts);

    /// <summary>
    /// Called immediately before a rule fires.
    /// </summary>
    /// <param name="rule">The rule about to fire.</param>
    void FiringRule(IRule rule);

    /// <summary>
    /// Called immediately after a rule has fired.
    /// </summary>
    /// <param name="rule">The rule that fired.</param>
    /// <param name="modified">Whether the rule modified the fact space (e.g. inserted, modified, or retracted a fact).</param>
    void FiredRule(IRule rule, bool modified);

    /// <summary>
    /// Called when evaluation of a specific fact tuple ends.
    /// </summary>
    /// <param name="facts">The array of fact objects that were evaluated.</param>
    void EndTupleEvaluation(object[] facts);

    /// <summary>
    /// Called when a rule evaluation session ends.
    /// </summary>
    void EndEvaluation();

    /// <summary>
    /// Called when a <see cref="RuleEvent"/> is created during evaluation.
    /// </summary>
    /// <param name="evalEvent">The rule event that was created.</param>
    void EventCreated(RuleEvent evalEvent);

    /// <summary>
    /// Logs a debug-level message using the specified template and markers.
    /// </summary>
    /// <param name="template">The message template.</param>
    /// <param name="markers">The values to substitute into the template.</param>
    void Debug(string template, params object[] markers);

    /// <summary>
    /// Logs a warning-level message using the specified template and markers.
    /// </summary>
    /// <param name="template">The message template.</param>
    /// <param name="markers">The values to substitute into the template.</param>
    void Warning(string template, params object[] markers);

}

