using CommunityToolkit.Diagnostics;

namespace Pondhawk.Utilities.Pipeline;

public abstract class BasePipelineStep<TContext> where TContext : class, IPipelineContext
{

    public bool ContinueAfterFailure { get; set; }
    
    public async Task InvokeAsync(TContext context, Func<TContext, Task> next)
    {

        Guard.IsNotNull(context, nameof(context));
        Guard.IsNotNull(next, nameof(next));
        
        if (!ContinueAfterFailure && !context.Success)
            return;

        
        await Before(context);
        
        await next(context);
        
        if (!ContinueAfterFailure && !context.Success )
            return;
        
        await After(context);        

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