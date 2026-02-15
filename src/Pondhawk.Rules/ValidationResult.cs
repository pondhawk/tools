namespace Pondhawk.Rules;


/// <summary>
/// The result of validating one or more entities through a rule set, containing violations grouped by category.
/// </summary>
public sealed class ValidationResult
{
    public bool IsValid { get; init; }
    public EvaluationResults Results { get; init; }
    public IReadOnlyList<RuleEvent> Violations { get; init; }
    public IReadOnlyDictionary<string, List<RuleEvent>> ViolationsByGroup { get; init; }
}
