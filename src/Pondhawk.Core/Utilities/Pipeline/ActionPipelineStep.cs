namespace Pondhawk.Utilities.Pipeline;

internal class ActionPipelineStep<TContext>( Func<TContext,Task> action ): IPipelineStep<TContext> where TContext : class, IPipelineContext
{

    public bool ContinueAfterFailure { get; set; }

    public async Task InvokeAsync( TContext context, Func<TContext,Task> next )
    {

        await action(context);
        context.Phase = PipelinePhase.After;

    }


}
