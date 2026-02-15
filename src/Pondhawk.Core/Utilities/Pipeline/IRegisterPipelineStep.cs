namespace Pondhawk.Utilities.Pipeline;

public interface IRegisterPipelineStep<TContext> where TContext : class, IPipelineContext
{
    /// <summary>
    /// Adds a pipeline step of the specified type to the current pipeline registration.
    /// </summary>
    /// <typeparam name="TStep">The type of the pipeline step to be added, which must implement <see cref="IPipelineStep{TContext}"/>.</typeparam>
    /// <returns>The current <see cref="IRegisterPipelineStep{TContext}"/> instance for chaining further step registrations.</returns>
    IRegisterPipelineStep<TContext> Add<TStep>() where TStep : class, IPipelineStep<TContext>;
}
