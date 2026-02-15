namespace Pondhawk.Exceptions;

/// <summary>
/// Provides structured error information (kind, code, explanation, details) for exception types.
/// </summary>
public interface IExceptionInfo
{

    ErrorKind Kind { get; }
    string ErrorCode { get; }
    string Explanation { get; }

    IList<EventDetail> Details { get; }

}
