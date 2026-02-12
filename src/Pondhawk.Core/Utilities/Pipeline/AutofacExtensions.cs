using Autofac;
using CommunityToolkit.Diagnostics;

namespace Pondhawk.Utilities.Pipeline;

public static class AutofacExtensions
{

    /// <summary>
    /// Registers the pipeline factory with the Autofac container builder.
    /// </summary>
    /// <param name="builder">The Autofac <see cref="ContainerBuilder"/> used to register the pipeline factory.</param>
    /// <returns>The same <see cref="ContainerBuilder"/> instance for chaining registrations.</returns>
    public static ContainerBuilder RegisterPipelineFactory(this ContainerBuilder builder)
    {

        builder.RegisterType<PipelineFactory>()
            .As<IPipelineFactory>()
            .InstancePerLifetimeScope();
        
        return builder;
        
    }
    
    
    
    
    /// <summary>
    /// Registers a pipeline builder and its steps with the given container builder.
    /// </summary>
    /// <typeparam name="TContext">The type of the pipeline context that implements <see cref="IPipelineContext"/>.</typeparam>
    /// <param name="builder">The Autofac <see cref="ContainerBuilder"/> used to register pipeline components.</param>
    /// <param name="steps">An action to configure and register the pipeline steps.</param>
    /// <returns>The same <see cref="ContainerBuilder"/> instance for chaining registrations.</returns>
    public static ContainerBuilder AddPipeline<TContext>(this ContainerBuilder builder, Action<IRegisterPipelineStep<TContext>> steps ) where TContext : class, IPipelineContext
    {

        Guard.IsNotNull(builder, nameof(builder));
        Guard.IsNotNull(steps, nameof(steps));
        
        steps(new RegisterPipelineStep<TContext>(builder));
        
        builder.Register(c =>
            {

                var list = c.Resolve<IEnumerable<IPipelineStep<TContext>>>(); 
                var comp = new PipelineBuilder<TContext>();
                
                foreach( var step in list )
                {
                    comp.AddStep(step);
                }
                
                return comp;
                
            })
            .AsSelf()
            .As<IPipelineBuilder<TContext>>()
            .InstancePerDependency();

        return builder;
        
    }
    

}

public interface IRegisterPipelineStep<TContext> where TContext : class, IPipelineContext
{
    /// <summary>
    /// Adds a pipeline step of the specified type to the current pipeline registration.
    /// </summary>
    /// <typeparam name="TStep">The type of the pipeline step to be added, which must implement <see cref="IPipelineStep{TContext}"/>.</typeparam>
    /// <returns>The current <see cref="IRegisterPipelineStep{TContext}"/> instance for chaining further step registrations.</returns>
    IRegisterPipelineStep<TContext> Add<TStep>() where TStep : class, IPipelineStep<TContext>;
}

internal class RegisterPipelineStep<TContext>(ContainerBuilder builder): IRegisterPipelineStep<TContext> where TContext : class, IPipelineContext
{

    IRegisterPipelineStep<TContext> IRegisterPipelineStep<TContext>.Add<TStep>() => Add<TStep>();

    public IRegisterPipelineStep<TContext> Add<TStep>() where TStep : class, IPipelineStep<TContext>
    {

        Guard.IsNotNull(builder, nameof(builder));
        
        builder.RegisterType<TStep>()
            .As<IPipelineStep<TContext>>()
            .InstancePerDependency();
        
        return this;

    }
    
}