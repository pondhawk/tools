namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// A single step in a pipeline that wraps the next step in the chain.
/// </summary>
public interface IPipelineStep<TContext> where TContext : class
{

    bool ContinueAfterFailure { get; }
    Task InvokeAsync(TContext context, Func<TContext, Task> continuation);

}
