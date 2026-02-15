using System.Diagnostics.CodeAnalysis;
using Pondhawk.Rules;

namespace Pondhawk.Rules.Validators;
/// <summary>
/// Validation interface for collection properties, providing per-element and aggregate condition checking.
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not conflict with reserved keywords", Justification = "Is/IsNot/Otherwise(template) are intentional fluent API names that form the validation DSL and cannot be renamed without breaking the public API")]
public interface ICollectionValidator<out TFact, out TType>
{

    string GroupName { get; }
    string PropertyName { get; }

    ICollectionValidator<TFact, TType> Is(Func<TFact, IEnumerable<TType>, bool> condition);

    ICollectionValidator<TFact, TType> IsNot(Func<TFact, IEnumerable<TType>, bool> condition);

    IValidationRule<TFact> Otherwise(string template, params Func<TFact, object>[] parameters);

    IValidationRule<TFact> Otherwise(string group, string template, params Func<TFact, object>[] parameters);

    IValidationRule<TFact> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters);

}
