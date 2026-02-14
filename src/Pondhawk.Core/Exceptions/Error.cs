
namespace Pondhawk.Exceptions;


public class Ok
{
    
    public static readonly Ok Singleton = new ();

}

public class Error
{

    private static readonly List<EventDetail> EmptyDetails = [];
    public static readonly Error Ok = new() {Kind = ErrorKind.None, ErrorCode = "", Explanation = "", Details = [] };

    public ErrorKind Kind { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
    public IEnumerable<EventDetail> Details { get; init; } = EmptyDetails;

}


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
