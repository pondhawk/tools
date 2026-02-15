using Humanizer;
using Pondhawk.Rules;

namespace Pondhawk.Rules.Validators;


/// <summary>
/// Default implementation of <see cref="ICollectionValidator{TFact, TType}"/> for per-element collection validation.
/// </summary>
/// <param name="rule">The validation rule that owns this validator.</param>
/// <param name="group">The group name used to categorize violation messages.</param>
/// <param name="propertyName">The name of the collection property being validated.</param>
/// <param name="extractor">A function that extracts the collection from the fact.</param>
public class CollectionValidator<TFact, TType>(ValidationRule<TFact> rule, string group, string propertyName, Func<TFact, IEnumerable<TType>> extractor)
    : BaseValidator<TFact>(rule, group), ICollectionValidator<TFact, TType>
{
    /// <inheritdoc />
    public string GroupName { get; } = group;

    /// <inheritdoc />
    public string PropertyName { get; } = propertyName;

    /// <summary>
    /// Gets the function that extracts the collection from the fact.
    /// </summary>
    protected Func<TFact, IEnumerable<TType>> Extractor { get; } = extractor;


    /// <inheritdoc />
    public ICollectionValidator<TFact, TType> Is(Func<TFact, IEnumerable<TType>, bool> condition)
    {
        bool Cond(TFact f) => condition(f, Extractor(f));
        Conditions.Add(Cond);
        return this;
    }


    /// <inheritdoc />
    public ICollectionValidator<TFact, TType> IsNot(Func<TFact, IEnumerable<TType>, bool> condition)
    {
        bool Cond(TFact f) => !condition(f, Extractor(f));
        Conditions.Add(Cond);
        return this;
    }

}
