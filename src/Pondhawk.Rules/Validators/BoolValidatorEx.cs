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
/// Boolean validation extensions (e.g. Required, IsTrue, IsFalse).
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Ex suffix is an intentional domain convention for extension method classes")]
public static class BoolValidatorEx
{

    public static IValidator<TFact, bool> Required<TFact>(this IValidator<TFact, bool> validator) where TFact : class
    {
        var v = validator.Is((f, v) => v);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is required");

        return v;
    }

    public static IValidator<TFact, bool> IsTrue<TFact>(this IValidator<TFact, bool> validator) where TFact : class
    {

        var v = validator.Is((f, v) => v);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not true");

        return v;

    }

    public static IValidator<TFact, bool> IsFalse<TFact>(this IValidator<TFact, bool> validator) where TFact : class
    {

        var v = validator.IsNot((f, v) => v);

        var propName = validator.PropertyName.Humanize(LetterCasing.Title);

        v.Otherwise($"{propName} is not false");

        return v;

    }


}
