


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


/// <summary>
/// Convenience extension methods for <see cref="IRuleSet"/> providing simplified evaluation, validation, and decision APIs.
/// </summary>
/// <remarks>
/// <para>All extension methods suppress exceptions by default (unlike direct <c>IRuleSet.Evaluate(EvaluationContext)</c>).</para>
/// <para><c>Evaluate(facts)</c> — runs all matching rules and returns <see cref="EvaluationResults"/>.</para>
/// <para><c>TryValidate(subject, out violations)</c> — returns <c>true</c> if no violations; populates <c>violations</c> list otherwise.</para>
/// <para><c>Validate(facts)</c> — returns a <see cref="ValidationResult"/> with <c>IsValid</c>, <c>Violations</c>, and <c>ViolationsByGroup</c>.</para>
/// </remarks>
public static class RuleSetExtensions
{

    private static readonly List<RuleEvent> EmptyDetails = [];
    private static readonly Dictionary<string, List<RuleEvent>> EmptyGrouped = new();


    public static EvaluationResults Evaluate( this IRuleSet rules, params object[] facts )
    {

        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(facts);

        return rules.Evaluate(ec);

    }


    public static EvaluationResults Evaluate( this IRuleSet rules, IEnumerable<object> fact )
    {

        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(fact);

        return rules.Evaluate(ec);

    }


    public static bool TryValidate(this IRuleSet rules, object subject, out List<RuleEvent> violations)
    {

        violations = EmptyDetails;

        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddFacts(subject);

        var vr = rules.Evaluate(ec);

        if( !vr.HasViolations )
            return true;

        violations = [..vr.Events];

        return false;

    }


    public static bool TryValidate(this IRuleSet rules, IEnumerable<object> subjects, out List<RuleEvent> violations)
    {

        violations = EmptyDetails;

        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(subjects);

        var vr = rules.Evaluate(ec);

        if (!vr.HasViolations)
            return true;

        violations = [.. vr.Events];

        return false;

    }


    public static ValidationResult Validate(this IRuleSet rules, params object[] facts)
    {
        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(facts);

        var er = rules.Evaluate(ec);

        if (!er.HasViolations)
        {
            return new ValidationResult
            {
                IsValid = true,
                Results = er,
                Violations = EmptyDetails,
                ViolationsByGroup = EmptyGrouped
            };
        }

        var violations = er.GetViolations().ToList();
        var grouped = violations
            .GroupBy(e => e.Group ?? "")
            .ToDictionary(g => g.Key, g => g.ToList());

        return new ValidationResult
        {
            IsValid = false,
            Results = er,
            Violations = violations,
            ViolationsByGroup = grouped
        };
    }


}