namespace Pondhawk.Exceptions;

public class FailedValidationException: FluentException<FailedValidationException>
{
 
    public FailedValidationException(  IEnumerable<EventDetail> violations ) : base( "Violation events occurred during validation." )
    {
        WithKind(ErrorKind.Predicate);
        WithDetails(violations);
    }
    
}