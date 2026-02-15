
namespace Pondhawk.Exceptions;

/// <summary>
/// RFC 7807-style problem detail for HTTP API error responses, with correlation ID and event segments.
/// </summary>
public class ProblemDetail
{

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public string Detail { get; set; } = string.Empty;

    public string Instance { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;

    public IList<EventDetail> Segments { get; set; } = [];


}