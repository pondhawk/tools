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
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

namespace Pondhawk.Rules.Validators;

[UsedImplicitly]
public static class DateTimeValidatorEx
{

    public static IValidator<TFact, DateTime> Required<TFact>(this IValidator<TFact, DateTime> validator) where TFact : class
    {
        var v = validator.Is((_, value) => (value != DateTime.MinValue) && (value != DateTime.MaxValue));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is required");
        return v;
    }

    public static IValidator<TFact, DateTime> IsEqualTo<TFact>(this IValidator<TFact, DateTime> validator, DateTime test) where TFact : class
    {
        var v = validator.Is((_, value) => test.CompareTo(value) == 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must equal {test:yyyy-MM-dd HH:mm:ss}");
        return v;
    }

    public static IValidator<TFact, DateTime> IsEqualTo<TFact>(this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => extractor(f).CompareTo(value) == 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title); 
        v.Otherwise($"{propName} must equal the specified value");
        return v;
    }

    public static IValidator<TFact, DateTime> IsNotEqualTo<TFact>(this IValidator<TFact, DateTime> validator, DateTime test) where TFact : class
    {
        var v = validator.Is((_, value) => test.CompareTo(value) != 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal {test:yyyy-MM-dd HH:mm:ss}");
        return v;
    }

    public static IValidator<TFact, DateTime> IsNotEqualTo<TFact>(this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => extractor(f).CompareTo(value) != 0);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not equal the specified value");
        return v;
    }

    public static IValidator<TFact, DateTime> IsGreaterThen<TFact>(this IValidator<TFact, DateTime> validator, DateTime test) where TFact : class
    {
        var v = validator.Is((_, value) => value > test);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than {test:yyyy-MM-dd HH:mm:ss}");
        return v;
    }

    public static IValidator<TFact, DateTime> IsGreaterThen<TFact>(this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => value > extractor(f));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be greater than the specified value");
        return v;
    }

    public static IValidator<TFact, DateTime> IsLessThen<TFact>(this IValidator<TFact, DateTime> validator, DateTime test) where TFact : class
    {
        var v = validator.Is((_, value) => value < test);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than {test:yyyy-MM-dd HH:mm:ss}");
        return v;
    }

    public static IValidator<TFact, DateTime> IsLessThen<TFact>(this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> extractor) where TFact : class
    {
        var v = validator.Is((f, value) => value < extractor(f));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be less than the specified value");
        return v;
    }

    public static IValidator<TFact, DateTime> IsBetween<TFact>(this IValidator<TFact, DateTime> validator, DateTime low, DateTime high) where TFact : class
    {
        var v = validator.Is((_, value) => (value >= low) && (value <= high));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between {low:yyyy-MM-dd HH:mm:ss} and {high:yyyy-MM-dd HH:mm:ss}");
        return v;
    }

    public static IValidator<TFact, DateTime> IsBetween<TFact>(this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> lowExtractor, Func<TFact, DateTime> highExtractor) where TFact : class
    {
        var v = validator.Is((f, value) => (value >= lowExtractor(f)) && (value <= highExtractor(f)));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be between the specified values");
        return v;
    }

    public static IValidator<TFact, DateTime> IsNotBetween<TFact>(this IValidator<TFact, DateTime> validator, DateTime low, DateTime high) where TFact : class
    {
        var v = validator.Is((_, value) => (value < low) || (value > high));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be between {low:yyyy-MM-dd HH:mm:ss} and {high:yyyy-MM-dd HH:mm:ss}");
        return v;
    }

    public static IValidator<TFact, DateTime> IsNotBetween<TFact>(this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> lowExtractor, Func<TFact, DateTime> highExtractor) where TFact : class
    {
        var v = validator.Is((f, value) => (value < lowExtractor(f)) || (value > highExtractor(f)));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be between the specified values");
        return v;
    }

    public static IValidator<TFact, DateTime> IsDayOfWeek<TFact>(this IValidator<TFact, DateTime> validator, DayOfWeek test) where TFact : class
    {
        var v = validator.Is((_, value) => value.DayOfWeek == test);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be a {test}");
        return v;
    }

    public static IValidator<TFact, DateTime> IsWeekend<TFact>(this IValidator<TFact, DateTime> validator) where TFact : class
    {
        var v = validator.Is((_, value) => (value.DayOfWeek == DayOfWeek.Saturday) || (value.DayOfWeek == DayOfWeek.Sunday));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be a weekend day");
        return v;
    }

    public static IValidator<TFact, DateTime> IsWeekday<TFact>(this IValidator<TFact, DateTime> validator) where TFact : class
    {
        var v = validator.Is((_, value) => (value.DayOfWeek != DayOfWeek.Saturday) && (value.DayOfWeek != DayOfWeek.Sunday));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be a weekday");
        return v;
    }

    public static IValidator<TFact, DateTime> IsInFuture<TFact>(this IValidator<TFact, DateTime> validator) where TFact : class
    {
        var v = validator.Is((_, value) => value > DateTime.Now);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be in the future");
        return v;
    }

    public static IValidator<TFact, DateTime> IsInPast<TFact>(this IValidator<TFact, DateTime> validator) where TFact : class
    {
        var v = validator.Is((_, value) => value <= DateTime.Now);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be in the past or present");
        return v;
    }

    public static IValidator<TFact, DateTime> IsDaysInFuture<TFact>(this IValidator<TFact, DateTime> validator, int days) where TFact : class
    {
        var v = validator.Is((_, value) => (value - DateTime.Now).TotalDays >= days);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be at least {days} day{(days != 1 ? "s" : "")} in the future");
        return v;
    }

    public static IValidator<TFact, DateTime> IsDaysInPast<TFact>(this IValidator<TFact, DateTime> validator, int days) where TFact : class
    {
        var v = validator.Is((_, value) => (DateTime.Now - value).TotalDays >= days);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be at least {days} day{(days != 1 ? "s" : "")} in the past");
        return v;
    }


}
