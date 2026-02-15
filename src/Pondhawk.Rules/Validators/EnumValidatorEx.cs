using System.Diagnostics.CodeAnalysis;
using Humanizer;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Enum validation extensions (e.g. IsInEnum, IsNotDefault).
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Ex suffix is an intentional domain convention for extension method classes")]
public static class EnumValidatorEx
{

    /// <summary>
    /// Validates that the enum property has a defined value in the <typeparamref name="TEnum"/> enumeration.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TEnum> IsInEnum<TFact, TEnum>(this IValidator<TFact, TEnum> validator)
        where TFact : class where TEnum : struct, Enum
    {
        var v = validator.Is((f, value) => Enum.IsDefined(value));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be a valid {typeof(TEnum).Name} value");
        return v;
    }

    /// <summary>
    /// Validates that the string property is a valid name of the <typeparamref name="TEnum"/> enumeration.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TEnum">The enum type whose names to check against.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="ignoreCase">Whether to ignore case when parsing the enum name.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsEnumName<TFact, TEnum>(this IValidator<TFact, string> validator, bool ignoreCase = false)
        where TFact : class where TEnum : struct, Enum
    {
        var v = validator.Is((f, value) => !string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value, ignoreCase, out _));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be a valid {typeof(TEnum).Name} name");
        return v;
    }

}
