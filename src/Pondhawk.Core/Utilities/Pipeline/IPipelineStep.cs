namespace Pondhawk.Utilities.Pipeline;

public interface IPipelineStep<TContext> where TContext : class
{
    
    bool ContinueAfterFailure { get; }
    Task InvokeAsync(TContext context, Func<TContext, Task> next);
    
}