/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Humanizer;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Numeric validation extensions for comparable value types (e.g. Required, IsPositive, IsInRange).
/// </summary>
public static class NumericValidatorEx
{

    // ===== Generic validators for all comparable value types =====

    public static IValidator<TFact, TType> Required<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !EqualityComparer<TType>.Default.Equals(value, default));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is required");
        return v;
    }

    public static IValidator<TFact, TType> IsNotZero<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !EqualityComparer<TType>.Default.Equals(value, default));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be zero");
        return v;
    }

    public static IValidator<TFact, TType> IsZero<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => EqualityComparer<TType>.Default.Equals(value, default));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be zero");
        return v;
    }

    public static IValidator<TFact, TType> IsEqual<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => EqualityComparer<TType>.Default.Equals(value, test));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal {test}");
        return v;
    }

    public static IValidator<TFact, TType> IsEqual<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => EqualityComparer<TType>.Default.Equals(value, extractor(f)));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal the specified value");
        return v;
    }

    public static IValidator<TFact, TType> IsNotEqual<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !EqualityComparer<TType>.Default.Equals(value, test));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal {test}");
        return v;
    }

    public static IValidator<TFact, TType> IsNotEqual<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !EqualityComparer<TType>.Default.Equals(value, extractor(f)));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal the specified value");
        return v;
    }

    public static IValidator<TFact, TType> IsGreaterThan<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(test) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than {test}");
        return v;
    }

    public static IValidator<TFact, TType> IsGreaterThan<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(extractor(f)) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than the specified value");
        return v;
    }

    public static IValidator<TFact, TType> IsLessThan<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(test) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than {test}");
        return v;
    }

    public static IValidator<TFact, TType> IsLessThan<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(extractor(f)) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than the specified value");
        return v;
    }

    public static IValidator<TFact, TType> IsGreaterThanOrEqual<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(test) >= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than or equal to {test}");
        return v;
    }

    public static IValidator<TFact, TType> IsGreaterThanOrEqual<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(extractor(f)) >= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than or equal to the specified value");
        return v;
    }

    public static IValidator<TFact, TType> IsLessThanOrEqual<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(test) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than or equal to {test}");
        return v;
    }

    public static IValidator<TFact, TType> IsLessThanOrEqual<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(extractor(f)) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than or equal to the specified value");
        return v;
    }

    public static IValidator<TFact, TType> IsBetween<TFact, TType>(this IValidator<TFact, TType> validator, TType low, TType high)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(low) >= 0 && value.CompareTo(high) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between {low} and {high}");
        return v;
    }

    public static IValidator<TFact, TType> IsNotBetween<TFact, TType>(this IValidator<TFact, TType> validator, TType low, TType high)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(low) < 0 || value.CompareTo(high) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be between {low} and {high}");
        return v;
    }

    public static IValidator<TFact, TType> IsBetween<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> lowExtractor, Func<TFact, TType> highExtractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(lowExtractor(f)) >= 0 && value.CompareTo(highExtractor(f)) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between the specified values");
        return v;
    }

    public static IValidator<TFact, TType> IsNotBetween<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> lowExtractor, Func<TFact, TType> highExtractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(lowExtractor(f)) < 0 || value.CompareTo(highExtractor(f)) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be between the specified values");
        return v;
    }

    public static IValidator<TFact, TType> IsExclusiveBetween<TFact, TType>(this IValidator<TFact, TType> validator, TType low, TType high)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(low) > 0 && value.CompareTo(high) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between {low} and {high} (exclusive)");
        return v;
    }

    public static IValidator<TFact, TType> IsExclusiveBetween<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> lowExtractor, Func<TFact, TType> highExtractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(lowExtractor(f)) > 0 && value.CompareTo(highExtractor(f)) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between the specified values (exclusive)");
        return v;
    }

    public static IValidator<TFact, TType> IsIn<TFact, TType>(this IValidator<TFact, TType> validator, params TType[] values)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => values.Contains(value));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be one of the allowed values");
        return v;
    }

    public static IValidator<TFact, TType> IsNotIn<TFact, TType>(this IValidator<TFact, TType> validator, params TType[] values)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !values.Contains(value));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be one of the prohibited values");
        return v;
    }

    // ===== Numeric shortcuts =====

    public static IValidator<TFact, TType> IsPositive<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(default) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be positive");
        return v;
    }

    public static IValidator<TFact, TType> IsNegative<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(default) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be negative");
        return v;
    }

    public static IValidator<TFact, TType> IsNonNegative<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(default) >= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be negative");
        return v;
    }

    public static IValidator<TFact, TType> IsNonPositive<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(default) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be positive");
        return v;
    }


    // ===== Nullable type support =====

    public static IValidator<TFact, TType?> IsNull<TFact, TType>(this IValidator<TFact, TType?> validator)
        where TFact : class where TType : struct
    {
        var v = validator.Is((f, value) => !value.HasValue);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be null");
        return v;
    }

    public static IValidator<TFact, TType?> IsNotNull<TFact, TType>(this IValidator<TFact, TType?> validator)
        where TFact : class where TType : struct
    {
        var v = validator.Is((f, value) => value.HasValue);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be null");
        return v;
    }

    public static IValidator<TFact, TType?> HasValue<TFact, TType>(this IValidator<TFact, TType?> validator)
        where TFact : class where TType : struct
    {
        var v = validator.Is((f, value) => value.HasValue);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must have a value");
        return v;
    }


    // ===== Float-specific overloads (epsilon-based equality) =====

    private const float FloatTolerance = 1e-6f;

    public static IValidator<TFact, float> IsEqual<TFact>(this IValidator<TFact, float> validator, float test) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - test) < FloatTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal {test}");
        return v;
    }

    public static IValidator<TFact, float> IsEqual<TFact>(this IValidator<TFact, float> validator, Func<TFact, float> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - extractor(f)) < FloatTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal the specified value");
        return v;
    }

    public static IValidator<TFact, float> IsNotEqual<TFact>(this IValidator<TFact, float> validator, float test) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - test) >= FloatTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal {test}");
        return v;
    }

    public static IValidator<TFact, float> IsNotEqual<TFact>(this IValidator<TFact, float> validator, Func<TFact, float> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - extractor(f)) >= FloatTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal the specified value");
        return v;
    }

    public static IValidator<TFact, float> IsIn<TFact>(this IValidator<TFact, float> validator, params float[] values) where TFact : class
    {
        var v = validator.Is((f, value) => values.Any(val => Math.Abs(value - val) < FloatTolerance));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be one of the allowed values");
        return v;
    }

    public static IValidator<TFact, float> IsNotIn<TFact>(this IValidator<TFact, float> validator, params float[] values) where TFact : class
    {
        var v = validator.Is((f, value) => values.All(val => Math.Abs(value - val) >= FloatTolerance));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be one of the prohibited values");
        return v;
    }

    // ===== Double-specific overloads (epsilon-based equality) =====

    private const double DoubleTolerance = 1e-9;

    public static IValidator<TFact, double> IsEqual<TFact>(this IValidator<TFact, double> validator, double test) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - test) < DoubleTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal {test}");
        return v;
    }

    public static IValidator<TFact, double> IsEqual<TFact>(this IValidator<TFact, double> validator, Func<TFact, double> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - extractor(f)) < DoubleTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal the specified value");
        return v;
    }

    public static IValidator<TFact, double> IsNotEqual<TFact>(this IValidator<TFact, double> validator, double test) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - test) >= DoubleTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal {test}");
        return v;
    }

    public static IValidator<TFact, double> IsNotEqual<TFact>(this IValidator<TFact, double> validator, Func<TFact, double> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - extractor(f)) >= DoubleTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal the specified value");
        return v;
    }

    public static IValidator<TFact, double> IsIn<TFact>(this IValidator<TFact, double> validator, params double[] values) where TFact : class
    {
        var v = validator.Is((f, value) => values.Any(val => Math.Abs(value - val) < DoubleTolerance));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be one of the allowed values");
        return v;
    }

    public static IValidator<TFact, double> IsNotIn<TFact>(this IValidator<TFact, double> validator, params double[] values) where TFact : class
    {
        var v = validator.Is((f, value) => values.All(val => Math.Abs(value - val) >= DoubleTolerance));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be one of the prohibited values");
        return v;
    }


    // ===== Decimal precision/scale =====

    public static IValidator<TFact, decimal> HasPrecision<TFact>(this IValidator<TFact, decimal> validator, int precision, int scale, bool ignoreTrailingZeros = false) where TFact : class
    {
        var v = validator.Is((f, value) =>
        {
            var d = Math.Abs(value);
            if (ignoreTrailingZeros)
                d = d / 1.0000000000000000000000000000m;

            var bits = decimal.GetBits(d);
            var decimalPlaces = (bits[3] >> 16) & 0x7F;
            var unscaled = new decimal(bits[0], bits[1], bits[2], false, 0);
            var totalDigits = unscaled == 0m ? 1 : unscaled.ToString(System.Globalization.CultureInfo.InvariantCulture).Length;
            var integerDigits = totalDigits - decimalPlaces;
            var expectedIntDigits = precision - scale;

            return integerDigits <= expectedIntDigits && decimalPlaces <= scale;
        });
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must have no more than {precision} digits in total, with {scale} decimal places");
        return v;
    }
}
