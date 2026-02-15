namespace Pondhawk.Exceptions;

/// <summary>
/// An error indicating validation failures, carrying the associated violation details.
/// </summary>
public class NotValidError : Error
{

    public static NotValidError Create(IEnumerable<EventDetail> violations, string? context = null)
    {

        if (string.IsNullOrWhiteSpace(context))
            context = "No context available";

        var error = new NotValidError
        {
            Kind = ErrorKind.Predicate,
            ErrorCode = "ValidationFailure",
            Explanation = $"Validation errors exist. {context}",
            Details = [.. violations]
        };

        return error;

    }

}
