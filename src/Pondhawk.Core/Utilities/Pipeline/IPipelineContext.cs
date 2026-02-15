namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Indicates whether a pipeline step is executing before or after the main action.
/// </summary>
public enum PipelinePhase { Before, After }

/// <summary>
/// Context shared across pipeline steps, tracking success state and failure information.
/// </summary>
public interface IPipelineContext
{
    
    bool Success { get; set; }
    
    PipelinePhase Phase { get; set; }
    
    string FailedStep { get; set; }
    Exception? Cause { get; set; }
    
}