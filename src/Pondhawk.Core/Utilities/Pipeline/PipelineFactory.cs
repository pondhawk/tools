using Autofac;

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Autofac-based <see cref="IPipelineFactory"/> that resolves pipeline builders from the container.
/// </summary>
public class PipelineFactory(ILifetimeScope scope): IPipelineFactory
{


    public Pipeline<TContext> Create<TContext>() where TContext : class, IPipelineContext
    {

        var builder = scope.Resolve<IPipelineBuilder<TContext>>();
        var pipeline = builder.Build();

        return pipeline;

    }

}
