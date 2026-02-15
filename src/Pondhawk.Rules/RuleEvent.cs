namespace Pondhawk.Rules;

/// <summary>
/// An event emitted by a rule consequence — informational message, warning, or validation violation.
/// </summary>
public sealed class RuleEvent : IEquatable<RuleEvent>
{

    public enum EventCategory
    {
        Info,
        Warning,
        Violation
    }

    public EventCategory Category { get; init; } = EventCategory.Info;
    public string RuleName { get; init; } = "";
    public string Group { get; init; } = "";
    public string MessageTemplate { get; init; } = "";
    public string Message { get; init; } = "";


    public bool Equals(RuleEvent other)
    {
        if (other is null)
            return false;

        return Category == other.Category &&
               string.Equals(RuleName, other.RuleName, StringComparison.Ordinal) &&
               string.Equals(Group, other.Group, StringComparison.Ordinal) &&
               string.Equals(Message, other.Message, StringComparison.Ordinal);
    }

    public override bool Equals(object obj) => Equals(obj as RuleEvent);

    public override int GetHashCode() => HashCode.Combine(Category, RuleName, Group, Message);

}
