using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Utilities.Types
{


    /// <summary>
    /// Represents a named date/time range with begin and end boundaries and Unix timestamps.
    /// </summary>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "End is a domain property name for date range boundaries")]
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
