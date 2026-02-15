using System.Text.Json.Serialization;

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Base implementation of <see cref="IPipelineContext"/> with JSON-serializable success state and failure details.
/// </summary>
public abstract class BasePipelineContext
{

    public bool Success { get; set; } = true;

    [JsonConverter(typeof(JsonStringEnumConverter<PipelinePhase>))]
    public PipelinePhase Phase { get; set; }

    public string FailedStep { get; set; } = string.Empty;

    [JsonIgnore]
    public Exception? Cause { get; set; }

    public string ExceptionType => Cause?.GetType().Name ?? string.Empty;
    public string ExceptionMessage => Cause?.Message ?? string.Empty;

}
