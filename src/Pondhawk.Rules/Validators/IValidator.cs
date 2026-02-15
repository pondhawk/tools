using System.Diagnostics.CodeAnalysis;
using Pondhawk.Rules;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Fluent validation interface for asserting conditions on a property of type <typeparamref name="TType"/>
/// extracted from fact type <typeparamref name="TFact"/>. Use <c>Is()</c>/<c>IsNot()</c> to define conditions
/// and <c>Otherwise()</c> to specify the violation message.
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not conflict with reserved keywords", Justification = "Is/IsNot/Otherwise(template) are intentional fluent API names that form the validation DSL and cannot be renamed without breaking the public API")]
public interface IValidator<out TFact, out TType>
{

    string GroupName { get; }
    string PropertyName { get; }


    IValidator<TFact, TType> Is(Func<TFact, TType, bool> condition);

    IValidator<TFact, TType> IsNot(Func<TFact, TType, bool> condition);

    IValidationRule<TFact> Otherwise(string template, params Func<TFact, object>[] parameters);

    IValidationRule<TFact> Otherwise(string group, string template, params Func<TFact, object>[] parameters);

    IValidationRule<TFact> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters);


}
