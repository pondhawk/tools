namespace Pondhawk.Exceptions;

/// <summary>
/// An error wrapping an unhandled exception with an error code derived from the exception type.
/// </summary>
public class UnhandledError : Error
{

    public static UnhandledError Create(Exception cause, string? context = null)
    {

        if (string.IsNullOrWhiteSpace(context))
            context = "No context available";

        var errorCode = cause.GetType().Name.Replace("Exception", "");
        if (string.IsNullOrWhiteSpace(errorCode))
            errorCode = "Exception";

        var error = new UnhandledError
        {
            Kind = ErrorKind.System,
            ErrorCode = errorCode,
            Explanation = $"An unhandled exception was caught. {context}"
        };

        return error;

    }

}
