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

    /// <summary>
    /// Gets the group name used to categorize violation messages.
    /// </summary>
    string GroupName { get; }

    /// <summary>
    /// Gets the name of the property being validated.
    /// </summary>
    string PropertyName { get; }


    /// <summary>
    /// Adds a condition that must be true for the property value to be considered valid.
    /// </summary>
    /// <param name="condition">A function that receives the fact and the extracted property value and returns <c>true</c> if valid.</param>
    /// <returns>This validator instance for fluent chaining.</returns>
    IValidator<TFact, TType> Is(Func<TFact, TType, bool> condition);

    /// <summary>
    /// Adds a condition that must be false for the property value to be considered valid.
    /// </summary>
    /// <param name="condition">A function that receives the fact and the extracted property value and returns <c>true</c> for the negated condition.</param>
    /// <returns>This validator instance for fluent chaining.</returns>
    IValidator<TFact, TType> IsNot(Func<TFact, TType, bool> condition);

    /// <summary>
    /// Specifies the violation message to emit when the validation condition fails.
    /// </summary>
    /// <param name="template">A message template, optionally with format placeholders.</param>
    /// <param name="parameters">Functions that extract values from the fact to fill the template placeholders.</param>
    /// <returns>The validation rule for further configuration.</returns>
    IValidationRule<TFact> Otherwise(string template, params Func<TFact, object>[] parameters);

    /// <summary>
    /// Specifies the violation message and group name to emit when the validation condition fails.
    /// </summary>
    /// <param name="group">The group name for the violation event.</param>
    /// <param name="template">A message template, optionally with format placeholders.</param>
    /// <param name="parameters">Functions that extract values from the fact to fill the template placeholders.</param>
    /// <returns>The validation rule for further configuration.</returns>
    IValidationRule<TFact> Otherwise(string group, string template, params Func<TFact, object>[] parameters);

    /// <summary>
    /// Specifies the event category, group name, and violation message to emit when the validation condition fails.
    /// </summary>
    /// <param name="category">The event category (e.g. Violation, Warning, Info).</param>
    /// <param name="group">The group name for the violation event.</param>
    /// <param name="template">A message template, optionally with format placeholders.</param>
    /// <param name="parameters">Functions that extract values from the fact to fill the template placeholders.</param>
    /// <returns>The validation rule for further configuration.</returns>
    IValidationRule<TFact> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters);


}
