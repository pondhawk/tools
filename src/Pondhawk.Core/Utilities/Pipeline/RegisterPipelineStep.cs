using Autofac;
using CommunityToolkit.Diagnostics;

namespace Pondhawk.Utilities.Pipeline;

internal class RegisterPipelineStep<TContext>(ContainerBuilder builder) : IRegisterPipelineStep<TContext> where TContext : class, IPipelineContext
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
