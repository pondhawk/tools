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

using Pondhawk.Rules;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Default implementation of <see cref="IEnumerableValidator{TFact, TType}"/> for enumerable property validation.
/// </summary>
public class EnumerableValidator<TFact, TType>(ValidationRule<TFact> rule, string group, string propertyName, Func<TFact, IEnumerable<TType>> extractor)
    : BaseValidator<TFact>(rule, group), IEnumerableValidator<TFact, TType>
{
    public string GroupName { get; } = group;
    public string PropertyName { get; } = propertyName;

    protected Func<TFact, IEnumerable<TType>> Extractor { get; } = extractor;


    public IEnumerableValidator<TFact, TType> Is(Func<TFact, IEnumerable<TType>, bool> condition)
    {
        bool Cond(TFact f) => condition(f, Extractor(f));
        Conditions.Add(Cond);
        return this;
    }


    public IEnumerableValidator<TFact, TType> IsNot(Func<TFact, IEnumerable<TType>, bool> condition)
    {
        bool Cond(TFact f) => !condition(f, Extractor(f));
        Conditions.Add(Cond);
        return this;
    }
}
