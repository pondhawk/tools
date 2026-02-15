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
/// Default implementation of <see cref="IValidator{TFact, TType}"/> for single-value property validation.
/// </summary>
/// <param name="rule">The validation rule that owns this validator.</param>
/// <param name="group">The group name used to categorize violation messages.</param>
/// <param name="propertyName">The name of the property being validated.</param>
/// <param name="extractor">A function that extracts the property value from the fact.</param>
public class Validator<TFact, TType>(ValidationRule<TFact> rule, string group, string propertyName, Func<TFact, TType> extractor)
    : BaseValidator<TFact>(rule, group), IValidator<TFact, TType>
{
    /// <inheritdoc />
    public string GroupName { get; } = group;

    /// <inheritdoc />
    public string PropertyName { get; } = propertyName;

    /// <summary>
    /// Gets the function that extracts the property value from the fact.
    /// </summary>
    protected Func<TFact, TType> Extractor { get; } = extractor;


    /// <inheritdoc />
    public IValidator<TFact, TType> Is(Func<TFact, TType, bool> condition)
    {
        bool Cond(TFact f) => condition(f, Extractor(f));
        Conditions.Add(Cond);
        return this;
    }


    /// <inheritdoc />
    public IValidator<TFact, TType> IsNot(Func<TFact, TType, bool> condition)
    {
        bool Cond(TFact f) => !condition(f, Extractor(f));
        Conditions.Add(Cond);
        return this;
    }

}
