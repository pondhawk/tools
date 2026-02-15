namespace Pondhawk.Exceptions;

/// <summary>
/// An error indicating a requested resource was not found.
/// </summary>
public class NotFoundError : Error
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
