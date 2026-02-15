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
/// Enumerable validation extensions (e.g. Required, IsNotEmpty, HasMinCount).
/// </summary>
public static class EnumerableValidatorEx
{

    public static IEnumerableValidator<TFact, TType> Required<TFact, TType>(this IEnumerableValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} is required");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> IsEmpty<TFact, TType>(this IEnumerableValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => !value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must be empty");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> IsNotEmpty<TFact, TType>(this IEnumerableValidator<TFact, TType> validator) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Any());
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not be empty");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> Has<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Any(predicate));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at least one matching item");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> HasNone<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => !value.Any(predicate));
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must not contain any matching items");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> HasExactly<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Where(predicate).Take(count + 1).Count() == count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain exactly {count} matching item{(count != 1 ? "s" : "")}");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> HasOnlyOne<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Where(predicate).Take(2).Count() == 1);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain exactly one matching item");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> HasAtMostOne<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Where(predicate).Take(2).Count() <= 1);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at most one matching item");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> HasAtLeast<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Where(predicate).Take(count).Count() >= count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at least {count} matching item{(count != 1 ? "s" : "")}");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> HasAtMost<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Where(predicate).Take(count + 1).Count() <= count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain at most {count} matching item{(count != 1 ? "s" : "")}");
        return v;
    }


    // ===== Count-based assertions =====

    public static IEnumerableValidator<TFact, TType> HasCount<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, int count) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Take(count + 1).Count() == count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain exactly {count} item{(count != 1 ? "s" : "")}");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> HasCountGreaterThan<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, int count) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Take(count + 1).Count() > count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain more than {count} item{(count != 1 ? "s" : "")}");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> HasCountLessThan<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, int count) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => value.Take(count).Count() < count);
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain fewer than {count} item{(count != 1 ? "s" : "")}");
        return v;
    }

    public static IEnumerableValidator<TFact, TType> HasCountBetween<TFact, TType>(this IEnumerableValidator<TFact, TType> validator, int minimum, int maximum) where TFact : class where TType : class
    {
        var v = validator.Is((_, value) => { var c = value.Take(maximum + 1).Count(); return c >= minimum && c <= maximum; });
        var propName = validator.PropertyName.Humanize(LetterCasing.Title);
        v.Otherwise($"{propName} must contain between {minimum} and {maximum} items");
        return v;
    }

}