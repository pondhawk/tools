using Autofac;

namespace Pondhawk.Utilities.Pipeline;

public class PipelineFactory(ILifetimeScope scope): IPipelineFactory
{


    public Pipeline<TContext> Create<TContext>() where TContext : class, IPipelineContext
    {

        var builder = scope.Resolve<IPipelineBuilder<TContext>>();
        var pipeline = builder.Build();

        return pipeline;

    }

}
