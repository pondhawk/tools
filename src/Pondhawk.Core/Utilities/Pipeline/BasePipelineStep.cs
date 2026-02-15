using CommunityToolkit.Diagnostics;

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Base pipeline step with before/after hooks and automatic failure short-circuiting.
/// </summary>
public abstract class BasePipelineStep<TContext> where TContext : class, IPipelineContext
{

    public bool ContinueAfterFailure { get; set; }

    public async Task InvokeAsync(TContext context, Func<TContext, Task> continuation)
    {

        Guard.IsNotNull(context, nameof(context));
        Guard.IsNotNull(continuation, nameof(continuation));

        if (!ContinueAfterFailure && !context.Success)
            return;


        await Before(context).ConfigureAwait(false);

        await continuation(context).ConfigureAwait(false);

        if (!ContinueAfterFailure && !context.Success)
            return;

        await After(context).ConfigureAwait(false);

    }

    protected virtual Task Before(TContext context)
    {

        Guard.IsNotNull(context, nameof(context));

        return Task.CompletedTask;

    }

    protected virtual Task After(TContext context)
    {

        Guard.IsNotNull(context, nameof(context));

        return Task.CompletedTask;

    }


}
