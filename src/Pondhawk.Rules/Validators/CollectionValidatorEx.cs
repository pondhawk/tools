using System.Diagnostics.CodeAnalysis;
using Humanizer;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Collection validation extensions (e.g. Required, IsEmpty, Has, HasCount, HasCountBetween).
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Ex suffix is an intentional domain convention for extension method classes")]
public static class CollectionValidatorEx
{

    public static ICollectionValidator<TFact, TType> Required<TFact, TType>(this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is required");
        return v;
    }

    public static ICollectionValidator<TFact, TType> IsEmpty<TFact, TType>(this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.IsNot((f, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be empty");
        return v;
    }

    public static ICollectionValidator<TFact, TType> IsNotEmpty<TFact, TType>(this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
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
