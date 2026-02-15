using Humanizer;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Enum validation extensions (e.g. IsInEnum, IsNotDefault).
/// </summary>
public static class EnumValidatorEx
{

    public static IValidator<TFact, TEnum> IsInEnum<TFact, TEnum>(this IValidator<TFact, TEnum> validator)
        where TFact : class where TEnum : struct, Enum
    {
        var v = validator.Is((f, value) => Enum.IsDefined(value));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be a valid {typeof(TEnum).Name} value");
        return v;
    }

    public static IValidator<TFact, string> IsEnumName<TFact, TEnum>(this IValidator<TFact, string> validator, bool ignoreCase = false)
        where TFact : class where TEnum : struct, Enum
    {
        var v = validator.Is((f, value) => !string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value, ignoreCase, out _));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be a valid {typeof(TEnum).Name} name");
        return v;
    }

}
