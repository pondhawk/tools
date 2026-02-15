namespace Pondhawk.Utilities.Types
{

    
    /// <summary>
    /// Represents a named date/time range with begin and end boundaries and Unix timestamps.
    /// </summary>
    public interface IDateTimeRange
    {

        int Id { get; }

        DateTimeRange RangeKind { get; }

        string Label { get; }

        DateTime Begin { get; }
        long BeginTimestamp { get; }


        DateTime End { get; }
        long EndTimestamp { get; }
    }


}
