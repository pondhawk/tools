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

using System.Linq.Expressions;
using CommunityToolkit.Diagnostics;
using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Evaluation;
using Pondhawk.Rules.Util;
using Pondhawk.Rules.Validators;

namespace Pondhawk.Rules;

/// <summary>
/// A validation rule for fact type <typeparamref name="TFact"/> built using the <c>Assert().Is().Otherwise()</c> fluent API.
/// Runs at very high salience by default.
/// </summary>
public class ValidationRule<TFact> : AbstractRule, IValidationRule<TFact>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationRule{TFact}"/> class with the specified set and rule names.
    /// </summary>
    /// <param name="setName">The name of the rule set this validation rule belongs to.</param>
    /// <param name="ruleName">The name of this validation rule.</param>
    public ValidationRule(string setName, string ruleName) : base(setName, ruleName)
    {
        Salience = int.MaxValue - 1000000;

        OnlyFiresOnce = true;

        Predicates = [];
    }


    /// <summary>
    /// Gets the list of predicate functions that gate whether this validation rule is evaluated.
    /// </summary>
    protected IList<Func<TFact, bool>> Predicates { get; private set; }

    /// <summary>
    /// Gets the validator that holds the conditions and consequence for this rule.
    /// </summary>
    protected BaseValidator<TFact> TypeValidator { get; private set; }

    private Action<TFact> CascadeAction { get; set; }



    /// <summary>
    /// Sets the relative salience (priority) offset for this validation rule within the validation salience range.
    /// </summary>
    /// <param name="value">The salience offset to add to the base validation salience.</param>
    /// <returns>This rule instance for fluent chaining.</returns>
    public ValidationRule<TFact> WithSalience(int value)
    {
        Salience = (int.MaxValue - 1000000) + value;
        return this;
    }


    /// <summary>
    /// Places this validation rule in a mutex group so only the first failing rule in the group fires.
    /// </summary>
    /// <param name="name">The mutex group name.</param>
    /// <returns>This rule instance for fluent chaining.</returns>
    public ValidationRule<TFact> InMutex(string name)
    {
        Guard.IsNotNullOrWhiteSpace(name);

        Mutex = name;
        return this;
    }



    /// <summary>
    /// Sets the earliest date/time at which this validation rule becomes active.
    /// </summary>
    /// <param name="inception">The inception date/time.</param>
    /// <returns>This rule instance for fluent chaining.</returns>
    public ValidationRule<TFact> WithInception(DateTime inception)
    {
        Inception = inception;
        return this;
    }



    /// <summary>
    /// Sets the latest date/time at which this validation rule remains active.
    /// </summary>
    /// <param name="expiration">The expiration date/time.</param>
    /// <returns>This rule instance for fluent chaining.</returns>
    public ValidationRule<TFact> WithExpiration(DateTime expiration)
    {
        Expiration = expiration;
        return this;
    }


    /// <summary>
    /// Adds a precondition that must be true for this validation rule to be evaluated.
    /// </summary>
    /// <param name="predicate">A function that returns <c>true</c> if the rule should be evaluated.</param>
    /// <returns>This rule instance for fluent chaining.</returns>
    public ValidationRule<TFact> When(Func<TFact, bool> predicate)
    {
        Guard.IsNotNull(predicate);

        Predicates = [predicate];
        return this;
    }



    /// <summary>
    /// Adds an additional precondition that must also be true for this validation rule to be evaluated.
    /// </summary>
    /// <param name="predicate">A function that returns <c>true</c> if the rule should be evaluated.</param>
    /// <returns>This rule instance for fluent chaining.</returns>
    public ValidationRule<TFact> And(Func<TFact, bool> predicate)
    {
        Guard.IsNotNull(predicate);

        Predicates.Add(predicate);
        return this;
    }


    /// <summary>
    /// Adds a negated precondition: the rule is evaluated only when the predicate returns <c>false</c>.
    /// </summary>
    /// <param name="predicate">A function whose negation gates evaluation.</param>
    /// <returns>This rule instance for fluent chaining.</returns>
    public ValidationRule<TFact> Unless(Func<TFact, bool> predicate)
    {
        Guard.IsNotNull(predicate);

        Predicates = [f => !predicate(f)];
        return this;
    }



    /// <summary>
    /// Creates a validator for a scalar property extracted from the fact.
    /// </summary>
    /// <typeparam name="TType">The type of the property being validated.</typeparam>
    /// <param name="extractorEx">An expression that extracts the property value from the fact.</param>
    /// <returns>A validator for defining conditions on the extracted property.</returns>
    public IValidator<TFact, TType> Assert<TType>(Expression<Func<TFact, TType>> extractorEx)
    {
        Guard.IsNotNull(extractorEx);

        var factName = typeof(TFact).GetConciseName();

        var propName = "";
        var groupName = factName;
        if (extractorEx.Body is MemberExpression body)
        {
            propName = body.Member.Name;
            groupName = $"{factName}.{body.Member.Name}";

        }

        var extractor = extractorEx.Compile();

        var validator = new Validator<TFact, TType>(this, groupName, propName, extractor);
        TypeValidator = validator;

        return validator;

    }


    /// <summary>
    /// Creates a validator for an enumerable property extracted from the fact, enabling per-element validation.
    /// </summary>
    /// <typeparam name="TType">The element type of the enumerable property.</typeparam>
    /// <param name="extractorEx">An expression that extracts the enumerable from the fact.</param>
    /// <returns>An enumerable validator for defining conditions on the extracted collection.</returns>
    public EnumerableValidator<TFact, TType> AssertOver<TType>(Expression<Func<TFact, IEnumerable<TType>>> extractorEx)
    {
        Guard.IsNotNull(extractorEx);

        var factName = typeof(TFact).Name;

        var propName = "";
        var groupName = factName;
        if (extractorEx.Body is MemberExpression body)
        {
            propName = body.Member.Name;
            groupName = $"{factName}.{body.Member.Name}";

        }


        var extractor = extractorEx.Compile();

        var validator = new EnumerableValidator<TFact, TType>(this, groupName, propName, extractor);
        TypeValidator = validator;

        return validator;

    }


    /// <summary>
    /// Configures this rule to cascade a referenced object into the fact space for further validation.
    /// </summary>
    /// <typeparam name="TRef">The type of the referenced object to cascade.</typeparam>
    /// <param name="extractor">A function that extracts the referenced object from the fact.</param>
    public void Cascade<TRef>(Func<TFact, TRef> extractor) where TRef : class
    {
        Guard.IsNotNull(extractor);

        CascadeAction = f => RuleThreadLocalStorage.CurrentContext.InsertFact(extractor(f));
    }


    /// <summary>
    /// Configures this rule to cascade all elements of a child collection into the fact space for further validation.
    /// </summary>
    /// <typeparam name="TChild">The element type of the child collection to cascade.</typeparam>
    /// <param name="extractor">A function that extracts the child collection from the fact.</param>
    public void CascadeAll<TChild>(Func<TFact, IEnumerable<TChild>> extractor) where TChild : class
    {
        Guard.IsNotNull(extractor);

        CascadeAction = f => _CascadeCollection(extractor(f));
    }

    private static void _CascadeCollection(IEnumerable<object> children)
    {
        foreach (var o in children)
            RuleThreadLocalStorage.CurrentContext.InsertFact(o);
    }


    /// <inheritdoc />
    protected override IRule InternalEvaluate(object[] offered)
    {
        base.InternalEvaluate(offered);

        var fact = (TFact)offered[0];

        if (CascadeAction is not null)
            return this;

        if (Predicates.Count > 0 && Predicates.Any(cond => !cond(fact)))
            return null;

        if (TypeValidator.Conditions.Select(cond => cond(fact)).Any(result => !result))
            return this;

        return null;

    }


    /// <inheritdoc />
    protected override void InternalFire(object[] offered)
    {
        base.InternalFire(offered);

        var fact = (TFact)offered[0];

        if (CascadeAction is not null)
            CascadeAction(fact);
        else
            TypeValidator.Consequence(fact);
    }
}
