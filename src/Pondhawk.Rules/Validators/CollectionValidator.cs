using Humanizer;
using Pondhawk.Rules;

namespace Pondhawk.Rules.Validators;
/// <summary>
/// Validation interface for collection properties, providing per-element and aggregate condition checking.
/// </summary>
public interface ICollectionValidator<out TFact, out TType>
{

    string GroupName { get; }
    string PropertyName { get; }

    ICollectionValidator<TFact, TType> Is(Func<TFact, IEnumerable<TType>, bool> condition);

    ICollectionValidator<TFact, TType> IsNot(Func<TFact, IEnumerable<TType>, bool> condition);

    IValidationRule<TFact> Otherwise(string template, params Func<TFact, object>[] parameters);

    IValidationRule<TFact> Otherwise(string group, string template, params Func<TFact, object>[] parameters);

    IValidationRule<TFact> Otherwise( RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters);

}



/// <summary>
/// Default implementation of <see cref="ICollectionValidator{TFact, TType}"/> for per-element collection validation.
/// </summary>
public class CollectionValidator<TFact, TType>( ValidationRule<TFact> rule, string group, string propertyName, Func<TFact, IEnumerable<TType>> extractor )
    : BaseValidator<TFact>( rule, group ), ICollectionValidator<TFact, TType>
{
    public string GroupName { get; } = group;
    public string PropertyName { get; } = propertyName;

    protected Func<TFact, IEnumerable<TType>> Extractor { get; } = extractor;


    public ICollectionValidator<TFact, TType> Is(Func<TFact, IEnumerable<TType>, bool> condition)
    {
        bool Cond(TFact f) => condition(f, Extractor(f) );
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


/// <summary>
/// Collection validation extensions (e.g. Required, IsEmpty, Has, HasCount, HasCountBetween).
/// </summary>
public static class CollectionValidatorEx
{

    public static ICollectionValidator<TFact, TType> Required<TFact, TType>( this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is required");
        return v;
    }

    public static ICollectionValidator<TFact, TType> IsEmpty<TFact, TType>( this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.IsNot((f, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be empty");
        return v;
    }

    public static ICollectionValidator<TFact, TType> IsNotEmpty<TFact, TType>( this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be empty");
        return v;
    }



    public static ICollectionValidator<TFact, TType> Has<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class
        where TType : class
    {
        var v = validator.Is((f, value) => value.Any(predicate));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at least one matching item");
        return v;
    }



    public static ICollectionValidator<TFact, TType> HasNone<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.IsNot((f, value) => value.Any(predicate));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not contain any matching items");
        return v;
    }



    public static ICollectionValidator<TFact, TType> HasExactly<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(count + 1).Count() == count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain exactly {count} matching item{(count != 1 ? "s" : "")}");
        return v;
    }



    public static ICollectionValidator<TFact, TType> HasOnlyOne<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(2).Count() == 1);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain exactly one matching item");
        return v;
    }



    public static ICollectionValidator<TFact, TType> HasAtMostOne<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(2).Count() <= 1);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at most one matching item");
        return v;
    }



    public static ICollectionValidator<TFact, TType> HasAtLeast<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(count).Count() >= count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at least {count} matching item{(count != 1 ? "s" : "")}");
        return v;
    }



    public static ICollectionValidator<TFact, TType> HasAtMost<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(count + 1).Count() <= count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at most {count} matching item{(count != 1 ? "s" : "")}");
        return v;
    }


    // ===== Count-based assertions =====

    public static ICollectionValidator<TFact, TType> HasCount<TFact, TType>(this ICollectionValidator<TFact, TType> validator, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Take(count + 1).Count() == count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain exactly {count} item{(count != 1 ? "s" : "")}");
        return v;
    }

    public static ICollectionValidator<TFact, TType> HasCountGreaterThan<TFact, TType>(this ICollectionValidator<TFact, TType> validator, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Take(count + 1).Count() > count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain more than {count} item{(count != 1 ? "s" : "")}");
        return v;
    }

    public static ICollectionValidator<TFact, TType> HasCountLessThan<TFact, TType>(this ICollectionValidator<TFact, TType> validator, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Take(count).Count() < count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain fewer than {count} item{(count != 1 ? "s" : "")}");
        return v;
    }

    public static ICollectionValidator<TFact, TType> HasCountBetween<TFact, TType>(this ICollectionValidator<TFact, TType> validator, int minimum, int maximum) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => { var c = value.Take(maximum + 1).Count(); return c >= minimum && c <= maximum; });
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain between {minimum} and {maximum} items");
        return v;
    }

}
