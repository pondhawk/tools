namespace Pondhawk.Exceptions;

/// <summary>
/// Exception thrown when validation produces violation events, carrying the violation details.
/// </summary>
public class FailedValidationException : FluentException<FailedValidationException>
{

    public FailedValidationException(IEnumerable<EventDetail> violations) : base("Violation events occurred during validation.")
    {
        WithKind(ErrorKind.Predicate);
        WithDetails(violations);
    }

}
