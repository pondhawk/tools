using Pondhawk.Rules.Builder;

namespace Pondhawk.Rules;

/// <summary>
/// Marker interface for validation rules targeting fact type <typeparamref name="TFact"/>.
/// </summary>
public interface IValidationRule<out TFact> : IRule
{

}
