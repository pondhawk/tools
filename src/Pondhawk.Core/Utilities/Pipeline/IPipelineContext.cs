namespace Pondhawk.Utilities.Pipeline;

public enum PipelinePhase { Before, After }
public interface IPipelineContext
{
    
    bool Success { get; set; }
    
    PipelinePhase Phase { get; set; }
    
    string FailedStep { get; set; }
    Exception? Cause { get; set; }
    
}