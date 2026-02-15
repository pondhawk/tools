
namespace Pondhawk.Rql;

/// <summary>
/// Exception thrown for RQL parsing, serialization, or validation errors.
/// </summary>
public class RqlException: Exception
{

    public RqlException(string message) : base(message)
    {
    }

    public RqlException(string message, Exception inner) : base(message, inner)
    {
    }

}