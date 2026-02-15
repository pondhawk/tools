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
using System.Text.RegularExpressions;
using Humanizer;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// String-specific validation extensions (e.g. Required, MaxLength, MatchesPattern).
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Ex suffix is an intentional domain convention for extension method classes")]
public static class StringValidatorEx
{


    static StringValidatorEx()
    {

        _USStates = new HashSet<string>(["AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "DC"], StringComparer.Ordinal);

        _states = new HashSet<string>(["AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "DC", "PR", "VI", "AS", "GU", "MP", "AB", "BC", "MB", "NB", "NL", "NS", "ON", "PE", "QC", "SK", "NT", "NU", "YT"], StringComparer.Ordinal);


    }

    /// <summary>
    /// Validates that the object property is not null.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, object> Required<TFact>(this IValidator<TFact, object> validator) where TFact : class
    {
        var v = validator.Is((f, v) => v is not null);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is required");

        return v;

    }


    /// <summary>
    /// Validates that the string property is not null or whitespace.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> Required<TFact>(this IValidator<TFact, string> validator) where TFact : class
    {

        var v = validator.Is((f, v) => !string.IsNullOrWhiteSpace(v));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is required");

        return v;

    }


    /// <summary>
    /// Validates that the string property is null or whitespace (empty).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsEmpty<TFact>(this IValidator<TFact, string> validator) where TFact : class
    {
        var v = validator.Is((f, v) => string.IsNullOrWhiteSpace(v));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not Empty");

        return v;

    }

    /// <summary>
    /// Validates that the string property is not null or whitespace (not empty).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsNotEmpty<TFact>(this IValidator<TFact, string> validator) where TFact : class
    {
        var v = validator.Is((f, v) => !string.IsNullOrWhiteSpace(v));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is Empty");

        return v;


    }

    /// <summary>
    /// Validates that the string has at least the specified minimum length.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="minimum">The minimum allowed length.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> HasMinimumLength<TFact>(this IValidator<TFact, string> validator, int minimum) where TFact : class
    {

        var v = validator.Is((f, v) => !string.IsNullOrWhiteSpace(v) && v.Length >= minimum);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is too short");

        return v;

    }


    /// <summary>
    /// Validates that the string does not exceed the specified maximum length.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="maximum">The maximum allowed length.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> HasMaximumLength<TFact>(this IValidator<TFact, string> validator, int maximum) where TFact : class
    {

        var v = validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || v.Length <= maximum);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is too long");

        return v;

    }


    /// <summary>
    /// Validates that the string is one of the specified allowed values (ordinal comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="values">The allowed values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsIn<TFact>(this IValidator<TFact, string> validator, params string[] values) where TFact : class
    {

        var v = validator.Is((f, v) => values.Contains(v));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not in valid range ({string.Join(',', values)})");

        return v;

    }


    /// <summary>
    /// Validates that the string is not one of the specified prohibited values (ordinal comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="values">The prohibited values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsNotIn<TFact>(this IValidator<TFact, string> validator, params string[] values) where TFact : class
    {

        var v = validator.Is((f, v) => !values.Contains(v));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is in invalid range ({string.Join(',', values)})");

        return v;

    }


    /// <summary>
    /// Validates that the string is ordinally less than the specified value.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsLessThan<TFact>(this IValidator<TFact, string> validator, string test) where TFact : class
    {

        var v = validator.Is((f, v) => string.Compare(test, v, StringComparison.Ordinal) == 1);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not less than {test}");

        return v;

    }

    /// <summary>
    /// Validates that the string is ordinally less than a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsLessThan<TFact>(this IValidator<TFact, string> validator, Func<TFact, string> extractor) where TFact : class
    {

        var v = validator.Is((f, v) => string.Compare(extractor(f), v, StringComparison.Ordinal) == 1);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not less than");

        return v;

    }


    /// <summary>
    /// Validates that the string is ordinally greater than the specified value.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsGreaterThan<TFact>(this IValidator<TFact, string> validator, string test) where TFact : class
    {

        var v = validator.Is((f, v) => string.Compare(test, v, StringComparison.Ordinal) == -1);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not greater than {test}");

        return v;

    }

    /// <summary>
    /// Validates that the string is ordinally greater than a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsGreaterThan<TFact>(this IValidator<TFact, string> validator, Func<TFact, string> extractor) where TFact : class
    {
        var v = validator.Is((f, v) => string.Compare(extractor(f), v, StringComparison.Ordinal) == -1);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not greater than the specified value");

        return v;
    }

    /// <summary>
    /// Validates that the string is ordinally greater than or equal to the specified value.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsGreaterThanOrEqual<TFact>(this IValidator<TFact, string> validator, string test) where TFact : class
    {
        var v = validator.Is((f, v) => string.Compare(v, test, StringComparison.Ordinal) >= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than or equal to {test}");
        return v;
    }

    /// <summary>
    /// Validates that the string is ordinally greater than or equal to a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsGreaterThanOrEqual<TFact>(this IValidator<TFact, string> validator, Func<TFact, string> extractor) where TFact : class
    {
        var v = validator.Is((f, v) => string.Compare(v, extractor(f), StringComparison.Ordinal) >= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than or equal to the specified value");
        return v;
    }

    /// <summary>
    /// Validates that the string is ordinally less than or equal to the specified value.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsLessThanOrEqual<TFact>(this IValidator<TFact, string> validator, string test) where TFact : class
    {
        var v = validator.Is((f, v) => string.Compare(v, test, StringComparison.Ordinal) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than or equal to {test}");
        return v;
    }

    /// <summary>
    /// Validates that the string is ordinally less than or equal to a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsLessThanOrEqual<TFact>(this IValidator<TFact, string> validator, Func<TFact, string> extractor) where TFact : class
    {
        var v = validator.Is((f, v) => string.Compare(v, extractor(f), StringComparison.Ordinal) <= 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than or equal to the specified value");
        return v;
    }


    /// <summary>
    /// Validates that the string is ordinally equal to the specified value.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsEqualTo<TFact>(this IValidator<TFact, string> validator, string test) where TFact : class
    {

        var v = validator.Is((f, v) => string.Equals(test, v, StringComparison.Ordinal));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not equal to {test}");

        return v;

    }

    /// <summary>
    /// Validates that the string is ordinally equal to a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsEqualTo<TFact>(this IValidator<TFact, string> validator, Func<TFact, string> extractor) where TFact : class
    {

        var v = validator.Is((f, v) => string.Equals(extractor(f), v, StringComparison.Ordinal));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not equal to");

        return v;

    }


    /// <summary>
    /// Validates that the string is not ordinally equal to the specified value.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsNotEqualTo<TFact>(this IValidator<TFact, string> validator, string test) where TFact : class
    {

        var v = validator.Is((f, v) => !string.Equals(test, v, StringComparison.Ordinal));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is equal to {test}");

        return v;

    }

    /// <summary>
    /// Validates that the string is not ordinally equal to a value extracted from the fact.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="extractor">A function that extracts the comparison value from the fact.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsNotEqualTo<TFact>(this IValidator<TFact, string> validator, Func<TFact, string> extractor) where TFact : class
    {

        var v = validator.Is((f, v) => !string.Equals(extractor(f), v, StringComparison.Ordinal));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is equal to");

        return v;

    }


    /// <summary>
    /// Validates that the string matches the specified regular expression pattern.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsMatch<TFact>(this IValidator<TFact, string> validator, string pattern) where TFact : class
    {

        var v = validator.Is((f, v) => !string.IsNullOrWhiteSpace(v) && Regex.IsMatch(v, pattern, RegexOptions.None, TimeSpan.FromSeconds(1)));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} does not match {pattern}");

        return v;

    }

    /// <summary>
    /// Validates that the string does not match the specified regular expression pattern.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="pattern">The regular expression pattern that must not match.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsNotMatch<TFact>(this IValidator<TFact, string> validator, string pattern) where TFact : class
    {

        var v = validator.IsNot((f, v) => !string.IsNullOrWhiteSpace(v) && Regex.IsMatch(v, pattern, RegexOptions.None, TimeSpan.FromSeconds(1)));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} matches {pattern}");

        return v;

    }

    /// <summary>
    /// Validates that the string matches the specified compiled <see cref="Regex"/> instance.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="regex">The compiled regular expression to match.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsMatch<TFact>(this IValidator<TFact, string> validator, Regex regex) where TFact : class
    {
        var v = validator.Is((f, v) => !string.IsNullOrWhiteSpace(v) && regex.IsMatch(v));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} does not match the required pattern");
        return v;
    }

    /// <summary>
    /// Validates that the string does not match the specified compiled <see cref="Regex"/> instance.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="regex">The compiled regular expression that must not match.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsNotMatch<TFact>(this IValidator<TFact, string> validator, Regex regex) where TFact : class
    {
        var v = validator.IsNot((f, v) => !string.IsNullOrWhiteSpace(v) && regex.IsMatch(v));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} matches a prohibited pattern");
        return v;
    }

    /// <summary>
    /// Validates that the string is a valid North American phone number format.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsPhone<TFact>(this IValidator<TFact, string> validator) where TFact : class
    {

        var v = validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || Regex.IsMatch(v, @"\(?(\d{3})\)?-?(\d{3})-(\d{4})", RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1)));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not valid a Phone number");

        return v;

    }


    /// <summary>
    /// Validates that the string is a valid email address format.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsEmail<TFact>(this IValidator<TFact, string> validator) where TFact : class
    {

        var v = validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || Regex.IsMatch(v, @"^[a-zA-Z]+(([\'\,\.\- ][a-zA-Z ])?[a-zA-Z]*)*\s+&lt;(\w[-._\w]*\w@\w[-._\w]*\w\.\w{2,3})&gt;$|^(\w[-._\w]*\w@\w[-._\w]*\w\.\w{2,3})$", RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1)));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not valid a Email Address");

        return v;

    }


    /// <summary>
    /// Validates that the string is a valid US Social Security Number format (###-##-####).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsSsn<TFact>(this IValidator<TFact, string> validator) where TFact : class
    {

        var v = validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || Regex.IsMatch(v, @"^\d{3}-\d{2}-\d{4}$", RegexOptions.None, TimeSpan.FromSeconds(1)));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not valid a SSN");

        return v;

    }


    /// <summary>
    /// Validates that the string is a valid US zip code format (#####  or #####-####).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsZip<TFact>(this IValidator<TFact, string> validator) where TFact : class
    {

        var v = validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || Regex.IsMatch(v, @"^[0-9]{5}(?:-[0-9]{4})?$", RegexOptions.None, TimeSpan.FromSeconds(1)));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not valid a Zip Code");

        return v;


    }



    // ===== Case-insensitive variants =====

    /// <summary>
    /// Validates that the string equals the specified value using the given comparison type.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsEqualTo<TFact>(this IValidator<TFact, string> validator, string test, StringComparison comparison) where TFact : class
    {
        var v = validator.Is((f, v) => string.Equals(v, test, comparison));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is not equal to {test}");
        return v;
    }

    /// <summary>
    /// Validates that the string does not equal the specified value using the given comparison type.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="test">The value to compare against.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsNotEqualTo<TFact>(this IValidator<TFact, string> validator, string test, StringComparison comparison) where TFact : class
    {
        var v = validator.Is((f, v) => !string.Equals(v, test, comparison));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is equal to {test}");
        return v;
    }

    /// <summary>
    /// Validates that the string is one of the specified allowed values using the given comparison type.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <param name="values">The allowed values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsIn<TFact>(this IValidator<TFact, string> validator, StringComparison comparison, params string[] values) where TFact : class
    {
        var v = validator.Is((f, v) => values.Any(val => string.Equals(v, val, comparison)));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is not in valid range ({string.Join(',', values)})");
        return v;
    }

    /// <summary>
    /// Validates that the string is not one of the specified prohibited values using the given comparison type.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <param name="values">The prohibited values.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsNotIn<TFact>(this IValidator<TFact, string> validator, StringComparison comparison, params string[] values) where TFact : class
    {
        var v = validator.Is((f, v) => !values.Any(val => string.Equals(v, val, comparison)));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is in invalid range ({string.Join(',', values)})");
        return v;
    }


    // ===== Common text pattern validators =====

    /// <summary>
    /// Validates that the string starts with the specified prefix (ordinal comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="prefix">The required prefix.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> StartsWith<TFact>(this IValidator<TFact, string> validator, string prefix) where TFact : class
    {
        var v = validator.Is((f, v) => !string.IsNullOrEmpty(v) && v.StartsWith(prefix, StringComparison.Ordinal));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must start with '{prefix}'");
        return v;
    }

    /// <summary>
    /// Validates that the string starts with the specified prefix using the given comparison type.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="prefix">The required prefix.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> StartsWith<TFact>(this IValidator<TFact, string> validator, string prefix, StringComparison comparison) where TFact : class
    {
        var v = validator.Is((f, v) => !string.IsNullOrEmpty(v) && v.StartsWith(prefix, comparison));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must start with '{prefix}'");
        return v;
    }

    /// <summary>
    /// Validates that the string ends with the specified suffix (ordinal comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="suffix">The required suffix.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> EndsWith<TFact>(this IValidator<TFact, string> validator, string suffix) where TFact : class
    {
        var v = validator.Is((f, v) => !string.IsNullOrEmpty(v) && v.EndsWith(suffix, StringComparison.Ordinal));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must end with '{suffix}'");
        return v;
    }

    /// <summary>
    /// Validates that the string ends with the specified suffix using the given comparison type.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="suffix">The required suffix.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> EndsWith<TFact>(this IValidator<TFact, string> validator, string suffix, StringComparison comparison) where TFact : class
    {
        var v = validator.Is((f, v) => !string.IsNullOrEmpty(v) && v.EndsWith(suffix, comparison));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must end with '{suffix}'");
        return v;
    }

    /// <summary>
    /// Validates that the string contains the specified substring (ordinal comparison).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="substring">The required substring.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> Contains<TFact>(this IValidator<TFact, string> validator, string substring) where TFact : class
    {
        var v = validator.Is((f, v) => !string.IsNullOrEmpty(v) && v.Contains(substring, StringComparison.Ordinal));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain '{substring}'");
        return v;
    }

    /// <summary>
    /// Validates that the string contains the specified substring using the given comparison type.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="substring">The required substring.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> Contains<TFact>(this IValidator<TFact, string> validator, string substring, StringComparison comparison) where TFact : class
    {
        var v = validator.Is((f, v) => !string.IsNullOrEmpty(v) && v.Contains(substring, comparison));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain '{substring}'");
        return v;
    }

    /// <summary>
    /// Validates that the string length is between the specified minimum and maximum (inclusive).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <param name="minimum">The minimum allowed length (inclusive).</param>
    /// <param name="maximum">The maximum allowed length (inclusive).</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> HasLengthBetween<TFact>(this IValidator<TFact, string> validator, int minimum, int maximum) where TFact : class
    {
        var v = validator.Is((f, v) => !string.IsNullOrWhiteSpace(v) && v.Length >= minimum && v.Length <= maximum);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must have length between {minimum} and {maximum}");
        return v;
    }


    private static readonly HashSet<string> _USStates;


    /// <summary>
    /// Validates that the string is a valid two-letter US state abbreviation (50 states + DC).
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsUSState<TFact>(this IValidator<TFact, string> validator) where TFact : class
    {

        var v = validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || _USStates.Contains(v));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not a valid US State");

        return v;

    }


    private static readonly HashSet<string> _states;


    /// <summary>
    /// Validates that the string is a valid two-letter US state, territory, or Canadian province abbreviation.
    /// </summary>
    /// <typeparam name="TFact">The fact type.</typeparam>
    /// <param name="validator">The validator to extend.</param>
    /// <returns>The validator for fluent chaining.</returns>
    public static IValidator<TFact, string> IsState<TFact>(this IValidator<TFact, string> validator) where TFact : class
    {

        var v = validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || _states.Contains(v));

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not a valid State or Province");

        return v;

    }


}
