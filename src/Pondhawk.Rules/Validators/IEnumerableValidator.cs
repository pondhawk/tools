using System.Diagnostics.CodeAnalysis;
using Pondhawk.Rules;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Validation interface for enumerable properties, providing collection-level condition checking.
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not conflict with reserved keywords", Justification = "Is/IsNot/Otherwise(template) are intentional fluent API names that form the validation DSL and cannot be renamed without breaking the public API")]
public interface IEnumerableValidator<out TFact, out TType>
{


    string GroupName { get; }
    string PropertyName { get; }


    IEnumerableValidator<TFact, TType> Is(Func<TFact, IEnumerable<TType>, bool> condition);

    IEnumerableValidator<TFact, TType> IsNot(Func<TFact, IEnumerable<TType>, bool> condition);

    IValidationRule<TFact> Otherwise(string template, params Func<TFact, object>[] parameters);

    IValidationRule<TFact> Otherwise(string group, string template, params Func<TFact, object>[] parameters);

    IValidationRule<TFact> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters);

}
