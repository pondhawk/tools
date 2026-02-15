namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Indicates whether a pipeline step is executing before or after the main action.
/// </summary>
public enum PipelinePhase
{
    /// <summary>
    /// The pipeline is executing steps before the main action.
    /// </summary>
    Before,

    /// <summary>
    /// The pipeline is executing steps after the main action.
    /// </summary>
    After
}
