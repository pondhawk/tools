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

using System.Diagnostics.CodeAnalysis;
using Humanizer;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Numeric validation extensions for comparable value types (e.g. Required, IsPositive, IsInRange).
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Ex suffix is an intentional domain convention for extension method classes")]
public static class NumericValidatorEx
{

    // ===== Generic validators for all comparable value types =====

    /// <summary>
    /// Validates that the value is not the default for its type (e.g. not zero for numeric types).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> Required<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !EqualityComparer<TType>.Default.Equals(value, default));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is required");
        return v;
    }

    /// <summary>
    /// Validates that the value is not zero (not the default for its type).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsNotZero<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !EqualityComparer<TType>.Default.Equals(value, default));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be zero");
        return v;
    }

    /// <summary>
    /// Validates that the value is zero (the default for its type).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsZero<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => EqualityComparer<TType>.Default.Equals(value, default));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be zero");
        return v;
    }

    /// <summary>
    /// Validates that the value equals the specified constant.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsEqual<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => EqualityComparer<TType>.Default.Equals(value, test));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal {test}");
        return v;
    }

    /// <summary>
    /// Validates that the value equals a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsEqual<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => EqualityComparer<TType>.Default.Equals(value, extractor(f)));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the value does not equal the specified constant.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsNotEqual<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !EqualityComparer<TType>.Default.Equals(value, test));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal {test}");
        return v;
    }

    /// <summary>
    /// Validates that the value does not equal a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsNotEqual<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !EqualityComparer<TType>.Default.Equals(value, extractor(f)));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the value is strictly greater than the specified constant.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The lower bound (exclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsGreaterThan<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(test) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than {test}");
        return v;
    }

    /// <summary>
    /// Validates that the value is strictly greater than a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the lower bound from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsGreaterThan<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(extractor(f)) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the value is strictly less than the specified constant.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The upper bound (exclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsLessThan<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(test) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than {test}");
        return v;
    }

    /// <summary>
    /// Validates that the value is strictly less than a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the upper bound from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsLessThan<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(extractor(f)) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the value is greater than or equal to the specified constant.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The lower bound (inclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsGreaterThanOrEqual<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(test) >= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than or equal to {test}");
        return v;
    }

    /// <summary>
    /// Validates that the value is greater than or equal to a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the lower bound from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsGreaterThanOrEqual<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(extractor(f)) >= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than or equal to the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the value is less than or equal to the specified constant.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The upper bound (inclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsLessThanOrEqual<TFact, TType>(this IValidator<TFact, TType> validator, TType test)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(test) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than or equal to {test}");
        return v;
    }

    /// <summary>
    /// Validates that the value is less than or equal to a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the upper bound from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsLessThanOrEqual<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> extractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(extractor(f)) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than or equal to the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the value is between the specified low and high bounds (inclusive).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="low">The lower bound (inclusive).</param>
    /// <param name="high">The upper bound (inclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsBetween<TFact, TType>(this IValidator<TFact, TType> validator, TType low, TType high)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(low) >= 0 && value.CompareTo(high) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between {low} and {high}");
        return v;
    }

    /// <summary>
    /// Validates that the value is not between the specified low and high bounds (inclusive).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="low">The lower bound (inclusive).</param>
    /// <param name="high">The upper bound (inclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsNotBetween<TFact, TType>(this IValidator<TFact, TType> validator, TType low, TType high)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(low) < 0 || value.CompareTo(high) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be between {low} and {high}");
        return v;
    }

    /// <summary>
    /// Validates that the value is between bounds extracted from the fact (inclusive).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="lowExtractor">A function that extracts the lower bound from the fact.</param>
    /// <param name="highExtractor">A function that extracts the upper bound from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsBetween<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> lowExtractor, Func<TFact, TType> highExtractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(lowExtractor(f)) >= 0 && value.CompareTo(highExtractor(f)) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between the specified values");
        return v;
    }

    /// <summary>
    /// Validates that the value is not between bounds extracted from the fact (inclusive).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="lowExtractor">A function that extracts the lower bound from the fact.</param>
    /// <param name="highExtractor">A function that extracts the upper bound from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsNotBetween<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> lowExtractor, Func<TFact, TType> highExtractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(lowExtractor(f)) < 0 || value.CompareTo(highExtractor(f)) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be between the specified values");
        return v;
    }

    /// <summary>
    /// Validates that the value is between the specified low and high bounds (exclusive on both ends).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="low">The lower bound (exclusive).</param>
    /// <param name="high">The upper bound (exclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsExclusiveBetween<TFact, TType>(this IValidator<TFact, TType> validator, TType low, TType high)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(low) > 0 && value.CompareTo(high) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between {low} and {high} (exclusive)");
        return v;
    }

    /// <summary>
    /// Validates that the value is between bounds extracted from the fact (exclusive on both ends).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="lowExtractor">A function that extracts the lower bound from the fact.</param>
    /// <param name="highExtractor">A function that extracts the upper bound from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsExclusiveBetween<TFact, TType>(this IValidator<TFact, TType> validator, Func<TFact, TType> lowExtractor, Func<TFact, TType> highExtractor)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(lowExtractor(f)) > 0 && value.CompareTo(highExtractor(f)) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between the specified values (exclusive)");
        return v;
    }

    /// <summary>
    /// Validates that the value is one of the specified allowed values.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="values">The allowed values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsIn<TFact, TType>(this IValidator<TFact, TType> validator, params TType[] values)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => values.Contains(value));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be one of the allowed values");
        return v;
    }

    /// <summary>
    /// Validates that the value is not one of the specified prohibited values.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="values">The prohibited values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsNotIn<TFact, TType>(this IValidator<TFact, TType> validator, params TType[] values)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => !values.Contains(value));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be one of the prohibited values");
        return v;
    }

    // ===== Numeric shortcuts =====

    /// <summary>
    /// Validates that the value is strictly positive (greater than zero/default).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsPositive<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(default) > 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be positive");
        return v;
    }

    /// <summary>
    /// Validates that the value is strictly negative (less than zero/default).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsNegative<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(default) < 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be negative");
        return v;
    }

    /// <summary>
    /// Validates that the value is zero or positive (greater than or equal to zero/default).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsNonNegative<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(default) >= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be negative");
        return v;
    }

    /// <summary>
    /// Validates that the value is zero or negative (less than or equal to zero/default).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The comparable value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType> IsNonPositive<TFact, TType>(this IValidator<TFact, TType> validator)
        where TFact : class where TType : struct, IComparable<TType>
    {
        var v = validator.Is((f, value) => value.CompareTo(default) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be positive");
        return v;
    }


    // ===== Nullable type support =====

    /// <summary>
    /// Validates that the nullable value is null (has no value).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The underlying value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType?> IsNull<TFact, TType>(this IValidator<TFact, TType?> validator)
        where TFact : class where TType : struct
    {
        var v = validator.Is((f, value) => !value.HasValue);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be null");
        return v;
    }

    /// <summary>
    /// Validates that the nullable value is not null (has a value).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The underlying value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, TType?> IsNotNull<TFact, TType>(this IValidator<TFact, TType?> validator)
        where TFact : class where TType : struct
    {
        var v = validator.Is((f, value) => value.HasValue);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be null");
        return v;
    }

    /// <summary>
    /// Validates that the nullable value has a value (is not null).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <typeparam name="TType">The underlying value type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
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

    /// <summary>
    /// Validates that the float value equals the specified constant (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, float> IsEqual<TFact>(this IValidator<TFact, float> validator, float test) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - test) < FloatTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal {test}");
        return v;
    }

    /// <summary>
    /// Validates that the float value equals a value extracted from the fact (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, float> IsEqual<TFact>(this IValidator<TFact, float> validator, Func<TFact, float> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - extractor(f)) < FloatTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the float value does not equal the specified constant (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, float> IsNotEqual<TFact>(this IValidator<TFact, float> validator, float test) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - test) >= FloatTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal {test}");
        return v;
    }

    /// <summary>
    /// Validates that the float value does not equal a value extracted from the fact (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, float> IsNotEqual<TFact>(this IValidator<TFact, float> validator, Func<TFact, float> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - extractor(f)) >= FloatTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the float value is one of the specified allowed values (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="values">The allowed values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, float> IsIn<TFact>(this IValidator<TFact, float> validator, params float[] values) where TFact : class
    {
        var v = validator.Is((f, value) => values.Any(val => Math.Abs(value - val) < FloatTolerance));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be one of the allowed values");
        return v;
    }

    /// <summary>
    /// Validates that the float value is not one of the specified prohibited values (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="values">The prohibited values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, float> IsNotIn<TFact>(this IValidator<TFact, float> validator, params float[] values) where TFact : class
    {
        var v = validator.Is((f, value) => values.All(val => Math.Abs(value - val) >= FloatTolerance));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be one of the prohibited values");
        return v;
    }

    // ===== Double-specific overloads (epsilon-based equality) =====

    private const double DoubleTolerance = 1e-9;

    /// <summary>
    /// Validates that the double value equals the specified constant (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, double> IsEqual<TFact>(this IValidator<TFact, double> validator, double test) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - test) < DoubleTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal {test}");
        return v;
    }

    /// <summary>
    /// Validates that the double value equals a value extracted from the fact (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, double> IsEqual<TFact>(this IValidator<TFact, double> validator, Func<TFact, double> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - extractor(f)) < DoubleTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the double value does not equal the specified constant (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, double> IsNotEqual<TFact>(this IValidator<TFact, double> validator, double test) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - test) >= DoubleTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal {test}");
        return v;
    }

    /// <summary>
    /// Validates that the double value does not equal a value extracted from the fact (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, double> IsNotEqual<TFact>(this IValidator<TFact, double> validator, Func<TFact, double> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => Math.Abs(value - extractor(f)) >= DoubleTolerance);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the double value is one of the specified allowed values (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="values">The allowed values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, double> IsIn<TFact>(this IValidator<TFact, double> validator, params double[] values) where TFact : class
    {
        var v = validator.Is((f, value) => values.Any(val => Math.Abs(value - val) < DoubleTolerance));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be one of the allowed values");
        return v;
    }

    /// <summary>
    /// Validates that the double value is not one of the specified prohibited values (using epsilon-based comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="values">The prohibited values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, double> IsNotIn<TFact>(this IValidator<TFact, double> validator, params double[] values) where TFact : class
    {
        var v = validator.Is((f, value) => values.All(val => Math.Abs(value - val) >= DoubleTolerance));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be one of the prohibited values");
        return v;
    }


    // ===== Decimal precision/scale =====

    /// <summary>
    /// Validates that the decimal value conforms to the specified precision and scale constraints.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="precision">The maximum total number of digits.</param>
    /// <param name="scale">The maximum number of decimal places.</param>
    /// <param name="ignoreTrailingZeros">Whether to ignore trailing zeros when counting decimal places.</param>
    /// <returns>The validator for fluent chaining.</returns>
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
