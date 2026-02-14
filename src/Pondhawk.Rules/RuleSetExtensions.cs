

using Pondhawk.Exceptions;
using Pondhawk.Logging;

namespace Pondhawk.Rules;


public sealed class ValidationResult
{
    public bool IsValid { get; init; }
    public EvaluationResults Results { get; init; }
    public IReadOnlyList<EventDetail> Violations { get; init; }
    public IReadOnlyDictionary<string, List<EventDetail>> ViolationsByGroup { get; init; }
}


public static class RuleSetExtensions
{

    private static readonly List<EventDetail> EmptyDetails = [];
    private static readonly Dictionary<string, List<EventDetail>> EmptyGrouped = new();


    public static EvaluationResults Evaluate( this IRuleSet rules, params object[] facts )
    {

        var logger = rules.GetLogger();

        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(facts);

        var er = rules.Evaluate(ec);

        logger.LogObject(nameof(er), er);

        return er;

    }


    public static EvaluationResults Evaluate( this IRuleSet rules, IEnumerable<object> fact )
    {

        var logger = rules.GetLogger();

        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(fact);

        var er = rules.Evaluate(ec);

        logger.LogObject(nameof(er), er);

        return er;

    }


    public static bool TryValidate(this IRuleSet rules, object subject, out List<EventDetail> violations)
    {

        var logger = rules.GetLogger();


        violations = EmptyDetails;


        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddFacts(subject);

        var vr = rules.Evaluate(ec);

        logger.LogObject(nameof(vr), vr);


        if( !vr.HasViolations )
            return true;

        violations = [..vr.Events];

        return false;

    }


    public static bool TryValidate(this IRuleSet rules, IEnumerable<object> subjects, out List<EventDetail> violations)
    {

        var logger = rules.GetLogger();


        violations = EmptyDetails;


        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(subjects);

        var vr = rules.Evaluate(ec);

        logger.LogObject(nameof(vr), vr);


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