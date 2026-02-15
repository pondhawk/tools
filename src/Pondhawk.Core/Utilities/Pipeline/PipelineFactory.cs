using Microsoft.Extensions.DependencyInjection;

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// <see cref="IPipelineFactory"/> that resolves pipeline builders from the service provider.
/// </summary>
public class PipelineFactory(IServiceProvider serviceProvider) : IPipelineFactory
{


    /// <inheritdoc />
    public Pipeline<TContext> Create<TContext>() where TContext : class, IPipelineContext
    {

        var builder = serviceProvider.GetRequiredService<IPipelineBuilder<TContext>>();
        var pipeline = builder.Build();

        return pipeline;

    }

}
