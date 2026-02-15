namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Factory for creating typed pipeline instances from registered pipeline builders.
/// </summary>
public interface IPipelineFactory
{
    Pipeline<TContext> Create<TContext>() where TContext : class, IPipelineContext;
}