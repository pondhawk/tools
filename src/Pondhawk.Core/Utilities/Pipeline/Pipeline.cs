using CommunityToolkit.Diagnostics;
using Pondhawk.Utilities.Types;

namespace Pondhawk.Utilities.Pipeline;

public class Pipeline<TContext> where TContext : class, IPipelineContext
{

    private readonly ICollection<IPipelineStep<TContext>> _steps;

    internal Pipeline(ICollection<IPipelineStep<TContext>> steps )
    {
        _steps = steps;
    }

    public async Task ExecuteAsync( TContext context, Func<TContext, Task> action )
    {

        Guard.IsNotNull(context, nameof(context));

        var innerSteps = _steps.ToList();
        innerSteps.Add(new ActionPipelineStep<TContext>(action));

        Func<TContext, Task> nextAction = (_) => Task.CompletedTask;

        foreach( var step in innerSteps.AsEnumerable().Reverse() )
        {
            var currentStep = step;
            var capturedNext = nextAction;
            nextAction = async (ctx) => await InvokeWrapper(currentStep, ctx, capturedNext);
        }

        await nextAction(context);

        async Task InvokeWrapper( IPipelineStep<TContext> step, TContext ctx, Func<TContext,Task> next )
        {
            try
            {
                await step.InvokeAsync(ctx, next);
            }
            catch( Exception cause )
            {
                if( ctx.Success )
                {
                    ctx.Success = false;
                    ctx.FailedStep = step.GetType().GetConciseFullName();
                    ctx.Cause = cause;
                }

                if (!step.ContinueAfterFailure)
                    throw;
            }
        }

    }

}
