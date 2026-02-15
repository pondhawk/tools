namespace Pondhawk.Rules;


/// <summary>
/// The result of validating one or more entities through a rule set, containing violations grouped by category.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation passed with no violations.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the full evaluation results including scores, events, and fired rules.
    /// </summary>
    public EvaluationResults Results { get; init; }

    /// <summary>
    /// Gets the list of violation events produced during validation.
    /// </summary>
    public IReadOnlyList<RuleEvent> Violations { get; init; }

    /// <summary>
    /// Gets the violation events grouped by their group name.
    /// </summary>
    public IReadOnlyDictionary<string, List<RuleEvent>> ViolationsByGroup { get; init; }
}
