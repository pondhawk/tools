namespace Pondhawk.Exceptions;

/// <summary>
/// An error indicating a requested resource was not found.
/// </summary>
public class NotFoundError : Error
{

    /// <summary>
    /// Creates a <see cref="NotFoundError"/> with the specified explanation.
    /// </summary>
    /// <param name="explanation">A human-readable explanation of what was not found.</param>
    /// <returns>A new <see cref="NotFoundError"/> instance.</returns>
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
