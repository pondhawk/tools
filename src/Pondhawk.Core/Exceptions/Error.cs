
using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Exceptions;

/// <summary>
/// Represents a structured error with a kind, code, explanation, and optional detail events.
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is a well-established domain type name")]
public class Error
{

    public static readonly Error Ok = new() { Kind = ErrorKind.None, ErrorCode = "", Explanation = "", Details = [] };

    public ErrorKind Kind { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
    public IEnumerable<EventDetail> Details { get; init; } = [];

}
