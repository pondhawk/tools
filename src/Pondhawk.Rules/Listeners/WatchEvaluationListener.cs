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

using Microsoft.Extensions.Logging;
using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Evaluation;

namespace Pondhawk.Rules.Listeners;


public sealed class WatchEvaluationListener: IEvaluationListener
{

    public WatchEvaluationListener( ILogger logger )
    {
        Logger = logger;
    }


    private ILogger Logger { get; }


    public void BeginEvaluation()
    {


        if( !Logger.IsEnabled( LogLevel.Debug ) )
            return;

        var context = RuleThreadLocalStorage.CurrentContext;

        Logger.LogDebug( "Begin Evaluation - ({ContextDescription})", context.Description );

        Logger.LogDebug( "Context: {@Context}", context );

    }

    public void BeginTupleEvaluation( object[] facts )
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        Logger.LogDebug( "Begin Tuple Evaluation" );

        for (int i = 0; i < facts.Length; i++)
            Logger.LogDebug("{FactType}[{Index}]: {@Fact}", facts[i].GetType().FullName, i, facts[i]);


    }

    public void FiringRule( IRule rule )
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        Logger.LogDebug("Rule Firing ({RuleName}): {@Rule}", rule.Name, rule);

    }

    public void FiredRule( IRule rule, bool modified )
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        Logger.LogDebug( "Rule Fired ({RuleName}). Modified fact? {Modified}", rule.Name, modified );

    }

    public void EndTupleEvaluation( object[] facts )
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        Logger.LogDebug( "End Tuple Evaluation" );

    }

    public void EndEvaluation()
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        var context = RuleThreadLocalStorage.CurrentContext;

        Logger.LogDebug( "Results: {@Results}", context.Results );

        Logger.LogDebug( "End Evaluation - ({ContextDescription})", context.Description );

    }

    public void EventCreated( RuleEvent evalEvent )
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        Logger.LogDebug( "Evaluation Event Created: {@EvalEvent}", evalEvent );

    }

    public void Debug( string template, params object[] markers )
    {

        if (!Logger.IsEnabled(LogLevel.Debug))
            return;

        Logger.LogDebug( template, markers );

    }

    public void Warning( string template, params object[] markers )
    {

        if (!Logger.IsEnabled(LogLevel.Warning))
            return;

        Logger.LogWarning( template, markers );

    }

}
