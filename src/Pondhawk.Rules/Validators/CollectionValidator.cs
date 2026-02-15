using Humanizer;
using Pondhawk.Rules;

namespace Pondhawk.Rules.Validators;


/// <summary>
/// Default implementation of <see cref="ICollectionValidator{TFact, TType}"/> for per-element collection validation.
/// </summary>
public class CollectionValidator<TFact, TType>(ValidationRule<TFact> rule, string group, string propertyName, Func<TFact, IEnumerable<TType>> extractor)
    : BaseValidator<TFact>(rule, group), ICollectionValidator<TFact, TType>
{
    public string GroupName { get; } = group;
    public string PropertyName { get; } = propertyName;

    protected Func<TFact, IEnumerable<TType>> Extractor { get; } = extractor;


    public ICollectionValidator<TFact, TType> Is(Func<TFact, IEnumerable<TType>, bool> condition)
    {
        bool Cond(TFact f) => condition(f, Extractor(f));
        Conditions.Add(Cond);
        return this;
    }


    public ICollectionValidator<TFact, TType> IsNot(Func<TFact, IEnumerable<TType>, bool> condition)
    {
        bool Cond(TFact f) => !condition(f, Extractor(f));
        Conditions.Add(Cond);
        return this;
    }

}
