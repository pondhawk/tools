namespace Pondhawk.Utilities.Types;

/// <summary>
/// Default implementation of <see cref="IDateTimeRange"/> that calculates begin/end from a <see cref="DateTimeRange"/> kind.
/// </summary>
public class DateTimeRangeModel: IDateTimeRange
{


    public int Id { get; set; }

    public string Label { get; set; } = "Today";

    public DateTimeRange RangeKind { get; set; } = DateTimeRange.Today;

    public DateTime Begin => DateTimeHelpers.CalculateRange(RangeKind).begin;
    public DateTime End => DateTimeHelpers.CalculateRange(RangeKind).end;


    int IDateTimeRange.Id => Id;
    string IDateTimeRange.Label => Label;
    DateTime IDateTimeRange.Begin => Begin;
    long IDateTimeRange.BeginTimestamp => (long)(Begin.ToUniversalTime() - DateTimeHelpers.Epoch).TotalSeconds;

    DateTime IDateTimeRange.End => End;
    long IDateTimeRange.EndTimestamp => (long)(End.ToUniversalTime() - DateTimeHelpers.Epoch).TotalSeconds;

}