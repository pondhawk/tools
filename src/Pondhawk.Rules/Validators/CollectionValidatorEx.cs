using System.Diagnostics.CodeAnalysis;
using Humanizer;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Collection validation extensions (e.g. Required, IsEmpty, Has, HasCount, HasCountBetween).
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Ex suffix is an intentional domain convention for extension method classes")]
public static class CollectionValidatorEx
{

    /// <summary>
    /// Validates that the collection contains at least one element.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> Required<TFact, TType>(this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is required");
        return v;
    }

    /// <summary>
    /// Validates that the collection is empty (contains no elements).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> IsEmpty<TFact, TType>(this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.IsNot((f, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be empty");
        return v;
    }

    /// <summary>
    /// Validates that the collection is not empty (contains at least one element).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> IsNotEmpty<TFact, TType>(this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be empty");
        return v;
    }



    /// <summary>
    /// Validates that the collection contains at least one element matching the predicate.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="predicate">A function that returns <c>true</c> for matching elements.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> Has<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class
        where TType : class
    {
        var v = validator.Is((f, value) => value.Any(predicate));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at least one matching item");
        return v;
    }



    /// <summary>
    /// Validates that the collection contains no elements matching the predicate.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="predicate">A function that returns <c>true</c> for matching elements.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasNone<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.IsNot((f, value) => value.Any(predicate));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not contain any matching items");
        return v;
    }



    /// <summary>
    /// Validates that the collection contains exactly the specified number of elements matching the predicate.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="predicate">A function that returns <c>true</c> for matching elements.</param>
    /// <param name="count">The exact number of matching elements required.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasExactly<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(count + 1).Count() == count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain exactly {count} matching item{(count != 1 ? "s" : "")}");
        return v;
    }



    /// <summary>
    /// Validates that the collection contains exactly one element matching the predicate.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="predicate">A function that returns <c>true</c> for matching elements.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasOnlyOne<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(2).Count() == 1);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain exactly one matching item");
        return v;
    }



    /// <summary>
    /// Validates that the collection contains at most one element matching the predicate.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="predicate">A function that returns <c>true</c> for matching elements.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasAtMostOne<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(2).Count() <= 1);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at most one matching item");
        return v;
    }



    /// <summary>
    /// Validates that the collection contains at least the specified number of elements matching the predicate.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="predicate">A function that returns <c>true</c> for matching elements.</param>
    /// <param name="count">The minimum number of matching elements required.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasAtLeast<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(count).Count() >= count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at least {count} matching item{(count != 1 ? "s" : "")}");
        return v;
    }



    /// <summary>
    /// Validates that the collection contains at most the specified number of elements matching the predicate.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="predicate">A function that returns <c>true</c> for matching elements.</param>
    /// <param name="count">The maximum number of matching elements allowed.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasAtMost<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Where(predicate).Take(count + 1).Count() <= count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at most {count} matching item{(count != 1 ? "s" : "")}");
        return v;
    }


    // ===== Count-based assertions =====

    /// <summary>
    /// Validates that the collection contains exactly the specified number of total elements.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="count">The exact number of elements required.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasCount<TFact, TType>(this ICollectionValidator<TFact, TType> validator, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Take(count + 1).Count() == count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain exactly {count} item{(count != 1 ? "s" : "")}");
        return v;
    }

    /// <summary>
    /// Validates that the collection contains more than the specified number of elements.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="count">The minimum count threshold (exclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasCountGreaterThan<TFact, TType>(this ICollectionValidator<TFact, TType> validator, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Take(count + 1).Count() > count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain more than {count} item{(count != 1 ? "s" : "")}");
        return v;
    }

    /// <summary>
    /// Validates that the collection contains fewer than the specified number of elements.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="count">The maximum count threshold (exclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasCountLessThan<TFact, TType>(this ICollectionValidator<TFact, TType> validator, int count) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => value.Take(count).Count() < count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain fewer than {count} item{(count != 1 ? "s" : "")}");
        return v;
    }

    /// <summary>
    /// Validates that the collection count is between the specified minimum and maximum (inclusive).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The element type of the collection.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="minimum">The minimum number of elements (inclusive).</param>
    /// <param name="maximum">The maximum number of elements (inclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static ICollectionValidator<TFact, TType> HasCountBetween<TFact, TType>(this ICollectionValidator<TFact, TType> validator, int minimum, int maximum) where TFact : class where TType : class
    {
        var v = validator.Is((f, value) => { var c = value.Take(maximum + 1).Count(); return c >= minimum && c <= maximum; });
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain between {minimum} and {maximum} items");
        return v;
    }

}
