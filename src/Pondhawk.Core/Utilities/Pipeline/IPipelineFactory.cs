namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Factory for creating typed pipeline instances from registered pipeline builders.
/// </summary>
public interface IPipelineFactory
{
    /// <summary>
    /// Creates a pipeline instance for the specified context type from registered pipeline builders.
    /// </summary>
    /// <typeparam name="TContext">The pipeline context type that implements <see cref="IPipelineContext"/>.</typeparam>
    /// <returns>A fully constructed <see cref="Pipeline{TContext}"/> ready for execution.</returns>
    Pipeline<TContext> Create<TContext>() where TContext : class, IPipelineContext;
}
