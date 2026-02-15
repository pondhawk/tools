namespace Pondhawk.Rules;

/// <summary>
/// An event emitted by a rule consequence — informational message, warning, or validation violation.
/// </summary>
public sealed class RuleEvent : IEquatable<RuleEvent>
{

    /// <summary>Categorizes the severity of a <see cref="RuleEvent"/>.</summary>
    public enum EventCategory
    {
        /// <summary>An informational event.</summary>
        Info,

        /// <summary>A warning event.</summary>
        Warning,

        /// <summary>A validation violation event.</summary>
        Violation
    }

    /// <summary>Gets the category of this event.</summary>
    public EventCategory Category { get; init; } = EventCategory.Info;

    /// <summary>Gets the name of the rule that emitted this event.</summary>
    public string RuleName { get; init; } = "";

    /// <summary>Gets the logical group this event belongs to.</summary>
    public string Group { get; init; } = "";

    /// <summary>Gets the unformatted message template for this event.</summary>
    public string MessageTemplate { get; init; } = "";

    /// <summary>Gets the formatted message for this event.</summary>
    public string Message { get; init; } = "";


    /// <summary>Determines whether this event is equal to another <see cref="RuleEvent"/>.</summary>
    /// <param name="other">The other event to compare with.</param>
    /// <returns><c>true</c> if the events are equal; otherwise <c>false</c>.</returns>
    public bool Equals(RuleEvent other)
    {
        if (other is null)
            return false;

        return Category == other.Category &&
               string.Equals(RuleName, other.RuleName, StringComparison.Ordinal) &&
               string.Equals(Group, other.Group, StringComparison.Ordinal) &&
               string.Equals(Message, other.Message, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object obj) => Equals(obj as RuleEvent);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Category, RuleName, Group, Message);

}
