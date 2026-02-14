namespace Pondhawk.Utilities.Types
{

    
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
