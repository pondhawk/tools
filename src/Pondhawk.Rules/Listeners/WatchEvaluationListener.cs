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

using Pondhawk.Exceptions;
using Pondhawk.Logging;
using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Evaluation;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Pondhawk.Rules.Listeners;


public sealed class WatchEvaluationListener: IEvaluationListener
{

    public WatchEvaluationListener( ILogger logger )
    {
        Logger = logger;
    }

    public WatchEvaluationListener(  string category )
    {
        Logger =  Log.ForContext(Constants.SourceContextPropertyName, category);
    }


    private ILogger Logger { get; }


    public void BeginEvaluation()
    {

        
        if( !Logger.IsEnabled( LogEventLevel.Debug ) )
            return;

        var context = RuleThreadLocalStorage.CurrentContext;            

        Logger.Debug( "Begin Evaluation - ({ContextDescription})", context.Description );

        Logger.LogObject( "Context", context );

    }

    public void BeginTupleEvaluation( object[] facts )
    {

        if (!Logger.IsEnabled(LogEventLevel.Debug))
            return;

        Logger.Debug( "Begin Tuple Evaluation" );

        for (int i = 0; i < facts.Length; i++)
            Logger.LogObject($"{facts[i].GetType().FullName}[{i}]", facts[i]);


    }

    public void FiringRule( IRule rule )
    {

        if (!(Logger.IsEnabled(LogEventLevel.Debug)))
            return;

        Logger.LogObject($"Rule Firing ({rule.Name})", rule );

    }

    public void FiredRule( IRule rule, bool modified )
    {

        if (!(Logger.IsEnabled(LogEventLevel.Debug)))
            return;

        Logger.Debug( "Rule Fired ({0}). Modified fact? {1}", rule.Name, modified );

    }

    public void EndTupleEvaluation( object[] facts )
    {

        if (!(Logger.IsEnabled(LogEventLevel.Debug)))
            return;

        Logger.Debug( "End Tuple Evaluation" );

    }

    public void EndEvaluation()
    {

        if (!(Logger.IsEnabled(LogEventLevel.Debug)))
            return;

        var context = RuleThreadLocalStorage.CurrentContext;                                    

        Logger.LogObject( "Results", context.Results );

        Logger.Debug( "End Evaluation - ({context.Description})", context.Description );

    }

    public void EventCreated( EventDetail evalEvent )
    {

        if (!(Logger.IsEnabled(LogEventLevel.Debug)))
            return;

        Logger.LogObject( "Evaluation Event Created", evalEvent );

    }

    public void Debug( string template, params object[] markers )
    {

        if (!(Logger.IsEnabled(LogEventLevel.Debug)))
            return;

        Logger.Debug( template, markers );

    }

    public void Warning( string template, params object[] markers )
    {

        if (!(Logger.IsEnabled(LogEventLevel.Warning)))
            return;
        
        Logger.Warning( template, markers );

    }

}

