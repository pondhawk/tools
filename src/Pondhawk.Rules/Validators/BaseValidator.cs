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

using Pondhawk.Rules.Evaluation;

namespace Pondhawk.Rules.Validators;

/// <summary>
/// Base class for validators, providing condition collection and <c>Otherwise</c> violation message builders.
/// </summary>
public abstract class BaseValidator<TFact>
{


    /// <summary>
    /// Initializes a new instance of the <see cref="BaseValidator{TFact}"/> class.
    /// </summary>
    /// <param name="rule">The validation rule that owns this validator.</param>
    /// <param name="group">The group name used to categorize violation messages.</param>
    protected BaseValidator(ValidationRule<TFact> rule, string group)
    {

        Rule = rule;
        Group = group;

        Conditions = [];
        Consequence = f => { };

    }


    private string Group { get; }

    private ValidationRule<TFact> Rule { get; }


    /// <summary>
    /// Gets the list of condition functions that must all return <c>true</c> for the validation to pass.
    /// </summary>
    public IList<Func<TFact, bool>> Conditions { get; }

    /// <summary>
    /// Gets the action to execute when the validation fails, which emits a violation event.
    /// </summary>
    public Action<TFact> Consequence { get; private set; }


    /// <summary>
    /// Specifies the violation message to emit when the validation condition fails.
    /// </summary>
    /// <param name="template">A message template, optionally with format placeholders.</param>
    /// <param name="parameters">Functions that extract values from the fact to fill the template placeholders.</param>
    /// <returns>The validation rule for further configuration.</returns>
    public IValidationRule<TFact> Otherwise(string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Violation, Group, template, parameters);
        return Rule;
    }

    /// <summary>
    /// Specifies the violation message and group name to emit when the validation condition fails.
    /// </summary>
    /// <param name="group">The group name for the violation event.</param>
    /// <param name="template">A message template, optionally with format placeholders.</param>
    /// <param name="parameters">Functions that extract values from the fact to fill the template placeholders.</param>
    /// <returns>The validation rule for further configuration.</returns>
    public IValidationRule<TFact> Otherwise(string group, string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Violation, group, template, parameters);
        return Rule;
    }

    /// <summary>
    /// Specifies the event category, group name, and violation message to emit when the validation condition fails.
    /// </summary>
    /// <param name="category">The event category (e.g. Violation, Warning, Info).</param>
    /// <param name="group">The group name for the violation event.</param>
    /// <param name="template">A message template, optionally with format placeholders.</param>
    /// <param name="parameters">Functions that extract values from the fact to fill the template placeholders.</param>
    /// <returns>The validation rule for further configuration.</returns>
    public IValidationRule<TFact> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, category, group, template, parameters);
        return Rule;
    }


    private static void _BuildMessage(TFact fact, RuleEvent.EventCategory category, string group, string template, Func<TFact, object>[] parameters)
    {
        int len = parameters.Length;

        var markers = new object[len];
        for (int i = 0; i < len; i++)
        {
            object o = parameters[i](fact) ?? "null";
            markers[i] = o;
        }

        string desc = len == 0 ? template : string.Format(System.Globalization.CultureInfo.InvariantCulture, template, markers);
        RuleThreadLocalStorage.CurrentContext.Event(category, group, desc);
    }
}
