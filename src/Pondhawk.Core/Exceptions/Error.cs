
namespace Pondhawk.Exceptions;

/// <summary>
/// Represents a structured error with a kind, code, explanation, and optional detail events.
/// </summary>
public class Error
{

    public static readonly Error Ok = new() {Kind = ErrorKind.None, ErrorCode = "", Explanation = "", Details = [] };

    public ErrorKind Kind { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
    public IEnumerable<EventDetail> Details { get; init; } = [];

}


/// <summary>
/// An error indicating a requested resource was not found.
/// </summary>
public class NotFoundError: Error
{

    public static NotFoundError Create(string explanation)
    {

        var error = new NotFoundError
        {
            Kind = ErrorKind.NotFound,
            ErrorCode = "Not Found",
            Explanation = explanation
        };

        return error;

    }

}


/// <summary>
/// An error indicating validation failures, carrying the associated violation details.
/// </summary>
public class NotValidError : Error
{

    public static NotValidError Create(IEnumerable<EventDetail> violations, string? context=null )
    {

        if( string.IsNullOrWhiteSpace(context))
            context = "No context available";
        
        var error = new NotValidError
        {
            Kind = ErrorKind.Predicate,
            ErrorCode = "ValidationFailure",
            Explanation = $"Validation errors exist. {context}",
            Details = [..violations]
        };

        return error;

    }

}


/// <summary>
/// An error wrapping an unhandled exception with an error code derived from the exception type.
/// </summary>
public class UnhandledError : Error
{

    public static UnhandledError Create( Exception cause, string? context=null )
    {

        if( string.IsNullOrWhiteSpace(context))
            context = "No context available";
        
        var errorCode = cause.GetType().Name.Replace("Exception", "");
        if (string.IsNullOrWhiteSpace(errorCode))
            errorCode = "Exception";

        var error = new UnhandledError
        {
            Kind        = ErrorKind.System,
            ErrorCode   = errorCode,
            Explanation = $"An unhandled exception was caught. {context}"
        };

        return error;

    }

}
