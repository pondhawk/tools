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
using Microsoft.Extensions.Logging;
using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Evaluation;

namespace Pondhawk.Rules.Listeners;

/// <summary>
/// An <see cref="IEvaluationListener"/> that logs rule evaluation events via <see cref="Microsoft.Extensions.Logging.ILogger"/>.
/// </summary>
public sealed partial class WatchEvaluationListener : IEvaluationListener
{

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchEvaluationListener"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger instance to write evaluation trace events to.</param>
    public WatchEvaluationListener(ILogger logger)
    {
        Logger = logger;
    }


    private ILogger Logger { get; }


    /// <inheritdoc />
    public void BeginEvaluation()
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        var context = RuleThreadLocalStorage.CurrentContext;

        LogBeginEvaluation(Logger, context.Description);
        LogContext(Logger, context);
    }

    /// <inheritdoc />
    public void BeginTupleEvaluation(object[] facts)
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        LogBeginTupleEvaluation(Logger);

        for (int i = 0; i < facts.Length; i++)
            LogFact(Logger, facts[i].GetType().FullName, i, facts[i]);

    }

    /// <inheritdoc />
    public void FiringRule(IRule rule)
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        LogFiringRule(Logger, rule.Name, rule);

    }

    /// <inheritdoc />
    public void FiredRule(IRule rule, bool modified)
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        LogFiredRule(Logger, rule.Name, modified);

    }

    /// <inheritdoc />
    public void EndTupleEvaluation(object[] facts)
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        LogEndTupleEvaluation(Logger);

    }

    /// <inheritdoc />
    public void EndEvaluation()
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        var context = RuleThreadLocalStorage.CurrentContext;

        LogResults(Logger, context.Results);
        LogEndEvaluation(Logger, context.Description);

    }

    /// <inheritdoc />
    public void EventCreated(RuleEvent evalEvent)
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        LogEventCreated(Logger, evalEvent);

    }

    /// <inheritdoc />
    [SuppressMessage("Usage", "CA1848:Use the LoggerMessage delegates for improved performance", Justification = "Template varies by caller and cannot be a compile-time constant")]
    [SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Template varies by caller — this is a pass-through logging method")]
    public void Debug(string template, params object[] markers)
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        Logger.LogDebug(template, markers);

    }

    /// <inheritdoc />
    [SuppressMessage("Usage", "CA1848:Use the LoggerMessage delegates for improved performance", Justification = "Template varies by caller and cannot be a compile-time constant")]
    [SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Template varies by caller — this is a pass-through logging method")]
    public void Warning(string template, params object[] markers)
    {

        if (!Logger.IsEnabled(LogLevel.Warning))
            return;

        Logger.LogWarning(template, markers);

    }


    [LoggerMessage(Level = LogLevel.Debug, Message = "Begin Evaluation - ({ContextDescription})")]
    private static partial void LogBeginEvaluation(ILogger logger, string contextDescription);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Context: {Context}")]
    private static partial void LogContext(ILogger logger, object context);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Begin Tuple Evaluation")]
    private static partial void LogBeginTupleEvaluation(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{FactType}[{Index}]: {Fact}")]
    private static partial void LogFact(ILogger logger, string factType, int index, object fact);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Rule Firing ({RuleName}): {Rule}")]
    private static partial void LogFiringRule(ILogger logger, string ruleName, object rule);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Rule Fired ({RuleName}). Modified fact? {Modified}")]
    private static partial void LogFiredRule(ILogger logger, string ruleName, bool modified);

    [LoggerMessage(Level = LogLevel.Debug, Message = "End Tuple Evaluation")]
    private static partial void LogEndTupleEvaluation(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Results: {Results}")]
    private static partial void LogResults(ILogger logger, object results);

    [LoggerMessage(Level = LogLevel.Debug, Message = "End Evaluation - ({ContextDescription})")]
    private static partial void LogEndEvaluation(ILogger logger, string contextDescription);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Evaluation Event Created: {EvalEvent}")]
    private static partial void LogEventCreated(ILogger logger, object evalEvent);

}
