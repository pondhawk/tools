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

using CommunityToolkit.Diagnostics;
using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Evaluation;

namespace Pondhawk.Rules;

/// <summary>
/// A concrete rule that evaluates conditions and fires consequences against a single fact type.
/// </summary>
/// <remarks>
/// <para>Fluent API: <c>If(predicate).And(predicate).Then(action)</c> — conditions are AND-joined; all must be true to fire.</para>
/// <para><c>Otherwise(action)</c> inverts condition evaluation — fires when conditions are false (negated rule).</para>
/// <para><c>Fire(action)</c> always fires (no conditions). <c>NoConsequence()</c> tracks evaluation but does nothing.</para>
/// <para><b>Scoring:</b> <c>ThenAffirm(weight)</c>/<c>ThenVeto(weight)</c> add to <see cref="EvaluationResults.Score"/> for use with <c>Decide()</c>.</para>
/// <para><b>Forward chaining:</b> <c>Modifies(func)</c> signals that the consequence modifies a fact, triggering re-evaluation.
/// <c>Cascade&lt;T&gt;(func)</c> inserts a new fact into the fact space. <c>CascadeAll&lt;T&gt;(func)</c> inserts a collection.</para>
/// <para><b>Mutex:</b> <c>InMutex(name)</c> groups rules; only the first matching rule in a mutex group fires.</para>
/// <para><b>Fire-once:</b> <c>FireOnce()</c> prevents a rule from firing more than once per evaluation session.</para>
/// <para><b>Time-windowing:</b> <c>WithInception(dt)</c>/<c>WithExpiration(dt)</c> restrict when a rule is active.</para>
/// </remarks>
public sealed class Rule<TFact> : AbstractRule
{
    /// <summary>Initializes a new instance of the <see cref="Rule{TFact}"/> class with the specified set name and rule name.</summary>
    /// <param name="setName">The namespace (set name) for this rule.</param>
    /// <param name="ruleName">The name of this rule.</param>
    public Rule(string setName, string ruleName) : base(setName, ruleName)
    {
        Negated = false;

        Conditions = [];
        Consequence = null;
    }

    /// <summary>Gets a value indicating whether this rule's conditions are negated (fires when conditions are false).</summary>
    public bool Negated { get; private set; }

    private Action<TFact> CascadeAction { get; set; }

    private List<Func<TFact, bool>> Conditions { get; set; }
    private Action<TFact> Consequence { get; set; }
    private Func<TFact, object> ModifyFunc { get; set; }


    /// <summary>Sets the salience (priority) for this rule.</summary>
    /// <param name="value">The salience value; higher values evaluate first.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> WithSalience(int value)
    {
        Salience = value;
        return this;
    }

    /// <summary>Assigns this rule to a mutex group; only one rule per group fires per tuple.</summary>
    /// <param name="name">The mutex group name.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> InMutex(string name)
    {
        Mutex = name;
        return this;
    }

    /// <summary>Sets the earliest date/time at which this rule becomes active.</summary>
    /// <param name="inception">The inception date/time.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> WithInception(DateTime inception)
    {
        Inception = inception;
        return this;
    }

    /// <summary>Sets the latest date/time at which this rule remains active.</summary>
    /// <param name="expiration">The expiration date/time.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> WithExpiration(DateTime expiration)
    {
        Expiration = expiration;
        return this;
    }

    /// <summary>Restricts this rule to fire at most once per evaluation session.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> FireOnce()
    {
        OnlyFiresOnce = true;
        return this;
    }

    /// <summary>Allows this rule to fire multiple times per evaluation session.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> FireAlways()
    {
        OnlyFiresOnce = false;
        return this;
    }

    /// <summary>Adds a condition that must be true for this rule to fire.</summary>
    /// <param name="oCondition">The condition predicate.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> If(Func<TFact, bool> oCondition)
    {
        Conditions.Add(oCondition);
        return this;
    }

    /// <summary>Adds an additional condition (AND-joined) that must be true for this rule to fire.</summary>
    /// <param name="oCondition">The condition predicate.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> And(Func<TFact, bool> oCondition)
    {
        Conditions.Add(oCondition);
        return this;
    }

    /// <summary>Sets a no-op consequence; the rule tracks evaluation but performs no action.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> NoConsequence()
    {
        Consequence = f => { };
        return this;
    }

    /// <summary>Sets the consequence action to execute when conditions are met.</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Then(Action<TFact> oConsequence)
    {
        Consequence = oConsequence;
        return this;
    }

    /// <summary>Sets the consequence to emit an informational event using the specified message template.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Then(string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, "", template, parameters);
        return this;
    }

    /// <summary>Sets the consequence to emit an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Then(string group, string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, group, template, parameters);
        return this;
    }

    /// <summary>Sets the consequence to emit an event with the specified category, group, and message template.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Then(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, category, group, template, parameters);
        return this;
    }

    /// <summary>Sets the consequence to add the specified weight to the affirmation score.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> ThenAffirm(int weight)
    {
        Consequence = f => HandleAffirm(weight);
        return this;
    }

    /// <summary>Sets the consequence to add the specified weight to the veto score.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> ThenVeto(int weight)
    {
        Consequence = s => HandleVeto(weight);
        return this;
    }

    /// <summary>Sets a consequence that always fires (no conditions).</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Fire(Action<TFact> oConsequence)
    {
        Conditions.Add(f => true);
        Consequence = oConsequence;
        return this;
    }

    /// <summary>Sets a consequence that always fires, emitting an informational event.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Fire(string template, params Func<TFact, object>[] parameters)
    {
        Conditions.Add(f => true);
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, "", template, parameters);
        return this;
    }

    /// <summary>Sets a consequence that always fires, emitting an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Fire(string group, string template, params Func<TFact, object>[] parameters)
    {
        Conditions.Add(f => true);
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, group, template, parameters);
        return this;
    }

    /// <summary>Sets a consequence that always fires, emitting an event with the specified category.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Fire(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters)
    {
        Conditions.Add(f => true);
        Consequence = f => _BuildMessage(f, category, group, template, parameters);
        return this;
    }

    /// <summary>Sets a consequence that always fires, adding the specified weight to the affirmation score.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> FireAffirm(int weight)
    {
        Conditions.Add(f => true);
        Consequence = f => HandleAffirm(weight);
        return this;
    }

    /// <summary>Sets a consequence that always fires, adding the specified weight to the veto score.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> FireVeto(int weight)
    {
        Conditions.Add(f => true);
        Consequence = f => HandleVeto(weight);
        return this;
    }

    /// <summary>Sets a negated consequence that fires when conditions are false.</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Otherwise(Action<TFact> oConsequence)
    {
        Negated = true;
        Consequence = oConsequence;
        return this;
    }

    /// <summary>Sets a negated consequence that emits an informational event when conditions are false.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Otherwise(string template, params Func<TFact, object>[] parameters)
    {
        Negated = true;
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, "", template, parameters);
        return this;
    }

    /// <summary>Sets a negated consequence that emits an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Otherwise(string group, string template, params Func<TFact, object>[] parameters)
    {
        Negated = true;
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, group, template, parameters);
        return this;
    }

    /// <summary>Sets a negated consequence that emits an event with the specified category when conditions are false.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters)
    {
        Negated = true;
        Consequence = f => _BuildMessage(f, category, group, template, parameters);
        return this;
    }

    /// <summary>Sets a negated consequence that adds the specified weight to the affirmation score when conditions are false.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> OtherwiseAffirm(int weight)
    {
        Negated = true;
        Consequence = f => HandleAffirm(weight);
        return this;
    }

    /// <summary>Sets a negated consequence that adds the specified weight to the veto score when conditions are false.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> OtherwiseVeto(int weight)
    {
        Negated = true;
        Consequence = f => HandleVeto(weight);
        return this;
    }


    private static void _BuildMessage(TFact fact, RuleEvent.EventCategory category, string group, string template, Func<TFact, object>[] parameters)
    {
        if (parameters.Length == 0)
        {
            RuleThreadLocalStorage.CurrentContext.Event(category, group, template, fact);
            return;
        }

        var markers = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var o = parameters[i](fact) ?? "null";
            markers[i] = o;
        }

        var desc = string.Format(System.Globalization.CultureInfo.InvariantCulture, template, markers);
        RuleThreadLocalStorage.CurrentContext.Event(category, group, desc, fact);
    }



    /// <summary>Signals that the consequence modifies the fact, triggering re-evaluation.</summary>
    /// <param name="modifyFunc">A function that returns the modified fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact> Modifies(Func<TFact, object> modifyFunc)
    {
        ModifyFunc = modifyFunc;
        return this;
    }

    /// <summary>Inserts a referenced object extracted from the fact into the fact space for forward chaining.</summary>
    /// <typeparam name="TRef">The type of the referenced object.</typeparam>
    /// <param name="extractor">A function that extracts the referenced object from the fact.</param>
    public void Cascade<TRef>(Func<TFact, TRef> extractor) where TRef : class
    {
        Guard.IsNotNull(extractor);

        CascadeAction = f => RuleThreadLocalStorage.CurrentContext.InsertFact(extractor(f));

    }

    /// <summary>Inserts all items from a collection extracted from the fact into the fact space for forward chaining.</summary>
    /// <typeparam name="TChild">The type of the child items.</typeparam>
    /// <param name="extractor">A function that extracts the child collection from the fact.</param>
    public void CascadeAll<TChild>(Func<TFact, IEnumerable<TChild>> extractor) where TChild : class
    {
        Guard.IsNotNull(extractor);

        CascadeAction = f => _CascadeCollection(extractor(f));
    }

    private static void _CascadeCollection(IEnumerable<object> children)
    {
        foreach (object o in children)
            RuleThreadLocalStorage.CurrentContext.InsertFact(o);
    }


    /// <inheritdoc />
    protected override IRule InternalEvaluate(object[] offered)
    {

        if (CascadeAction is not null)
            return this;


        base.InternalEvaluate(offered);

        var fact = (TFact)offered[0];



        foreach (var result in Conditions.Select(cond => cond(fact)))
        {
            if (result == Negated)
                return null;
        }

        return this;

    }


    /// <inheritdoc />
    protected override void InternalFire(object[] offered)
    {

        var fact = (TFact)offered[0];

        if (CascadeAction is not null)
        {
            CascadeAction(fact);
            return;
        }

        base.InternalFire(offered);

        Consequence(fact);

        if (ModifyFunc is not null)
            DispatchModify(ModifyFunc(fact));

    }

}


/// <summary>
/// A concrete rule that evaluates conditions and fires consequences against two fact types.
/// </summary>
/// <typeparam name="TFact1">The first fact type.</typeparam>
/// <typeparam name="TFact2">The second fact type.</typeparam>
public sealed class Rule<TFact1, TFact2> : AbstractRule
{

    /// <summary>Initializes a new instance of the <see cref="Rule{TFact1, TFact2}"/> class.</summary>
    /// <param name="setName">The namespace (set name) for this rule.</param>
    /// <param name="ruleName">The name of this rule.</param>
    public Rule(string setName, string ruleName) : base(setName, ruleName)
    {
        Negated = false;

        Conditions = [];
        Consequence = null;
    }

    /// <summary>Gets a value indicating whether this rule's conditions are negated (fires when conditions are false).</summary>
    public bool Negated { get; private set; }

    private Action<TFact1, TFact2> CascadeAction { get; set; }

    private List<Func<TFact1, TFact2, bool>> Conditions { get; set; }
    private Action<TFact1, TFact2> Consequence { get; set; }
    private Func<TFact1, TFact2, object> ModifyFunc { get; set; }

    /// <summary>Sets the salience (priority) for this rule.</summary>
    /// <param name="value">The salience value; higher values evaluate first.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> WithSalience(int value)
    {
        Salience = value;
        return this;
    }

    /// <summary>Assigns this rule to a mutex group; only one rule per group fires per tuple.</summary>
    /// <param name="name">The mutex group name.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> InMutex(string name)
    {
        Mutex = name;
        return this;
    }

    /// <summary>Sets the earliest date/time at which this rule becomes active.</summary>
    /// <param name="inception">The inception date/time.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> WithInception(DateTime inception)
    {
        Inception = inception;
        return this;
    }

    /// <summary>Sets the latest date/time at which this rule remains active.</summary>
    /// <param name="expiration">The expiration date/time.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> WithExpiration(DateTime expiration)
    {
        Expiration = expiration;
        return this;
    }

    /// <summary>Restricts this rule to fire at most once per evaluation session.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> FireOnce()
    {
        OnlyFiresOnce = true;
        return this;
    }

    /// <summary>Allows this rule to fire multiple times per evaluation session.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> FireAlways()
    {
        OnlyFiresOnce = false;
        return this;
    }

    /// <summary>Adds a condition that must be true for this rule to fire.</summary>
    /// <param name="oCondition">The condition predicate.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> If(Func<TFact1, TFact2, bool> oCondition)
    {
        Conditions.Add(oCondition);
        return this;
    }

    /// <summary>Adds an additional condition (AND-joined) that must be true for this rule to fire.</summary>
    /// <param name="oCondition">The condition predicate.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> And(Func<TFact1, TFact2, bool> oCondition)
    {
        Conditions.Add(oCondition);
        return this;
    }

    /// <summary>Sets a no-op consequence; the rule tracks evaluation but performs no action.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> NoConsequence()
    {
        Consequence = (f1, f2) => { };
        return this;
    }

    /// <summary>Sets the consequence action to execute when conditions are met.</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Then(Action<TFact1, TFact2> oConsequence)
    {
        Consequence = oConsequence;
        return this;
    }

    /// <summary>Sets the consequence to emit an informational event using the specified message template.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Then(string template, params Func<TFact1, TFact2, object>[] parameters)
    {
        Consequence = (f1, f2) => _BuildMessage(f1, f2, RuleEvent.EventCategory.Info, "", template, parameters);
        return this;
    }

    /// <summary>Sets the consequence to emit an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Then(string group, string template, params Func<TFact1, TFact2, object>[] parameters)
    {
        Consequence = (f1, f2) => _BuildMessage(f1, f2, RuleEvent.EventCategory.Info, group, template, parameters);
        return this;
    }

    /// <summary>Sets the consequence to emit an event with the specified category, group, and message template.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Then(RuleEvent.EventCategory category, string group, string template, params Func<TFact1, TFact2, object>[] parameters)
    {
        Consequence = (f1, f2) => _BuildMessage(f1, f2, category, group, template, parameters);
        return this;
    }

    /// <summary>Sets the consequence to add the specified weight to the affirmation score.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> ThenAffirm(int weight)
    {
        Consequence = (f1, f2) => HandleAffirm(weight);
        return this;
    }

    /// <summary>Sets the consequence to add the specified weight to the veto score.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> ThenVeto(int weight)
    {
        Consequence = (f1, f2) => HandleVeto(weight);
        return this;
    }

    /// <summary>Sets a consequence that always fires (no conditions).</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Fire(Action<TFact1, TFact2> oConsequence)
    {
        Conditions.Add((f1, f2) => true);
        Consequence = oConsequence;
        return this;
    }

    /// <summary>Sets a consequence that always fires, emitting an informational event.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Fire(string template, params Func<TFact1, TFact2, object>[] parameters)
    {
        Conditions.Add((f1, f2) => true);
        Consequence = (f1, f2) => _BuildMessage(f1, f2, RuleEvent.EventCategory.Info, "", template, parameters);
        return this;
    }

    /// <summary>Sets a consequence that always fires, emitting an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Fire(string group, string template, params Func<TFact1, TFact2, object>[] parameters)
    {
        Conditions.Add((f1, f2) => true);
        Consequence = (f1, f2) => _BuildMessage(f1, f2, RuleEvent.EventCategory.Info, group, template, parameters);
        return this;
    }

    /// <summary>Sets a consequence that always fires, emitting an event with the specified category.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Fire(RuleEvent.EventCategory category, string group, string template, params Func<TFact1, TFact2, object>[] parameters)
    {
        Conditions.Add((f1, f2) => true);
        Consequence = (f1, f2) => _BuildMessage(f1, f2, category, group, template, parameters);
        return this;
    }

    /// <summary>Sets a consequence that always fires, adding the specified weight to the affirmation score.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> FireAffirm(int weight)
    {
        Conditions.Add((f1, f2) => true);
        Consequence = (f1, f2) => HandleAffirm(weight);
        return this;
    }

    /// <summary>Sets a consequence that always fires, adding the specified weight to the veto score.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> FireVeto(int weight)
    {
        Conditions.Add((f1, f2) => true);
        Consequence = (f1, f2) => HandleVeto(weight);
        return this;
    }

    /// <summary>Sets a negated consequence that fires when conditions are false.</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Otherwise(Action<TFact1, TFact2> oConsequence)
    {
        Negated = true;
        Consequence = oConsequence;
        return this;
    }

    /// <summary>Sets a negated consequence that emits an informational event when conditions are false.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Otherwise(string template, params Func<TFact1, TFact2, object>[] parameters)
    {
        Negated = true;
        Consequence = (f1, f2) => _BuildMessage(f1, f2, RuleEvent.EventCategory.Info, "", template, parameters);
        return this;
    }

    /// <summary>Sets a negated consequence that emits an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Otherwise(string group, string template, params Func<TFact1, TFact2, object>[] parameters)
    {
        Negated = true;
        Consequence = (f1, f2) => _BuildMessage(f1, f2, RuleEvent.EventCategory.Info, group, template, parameters);
        return this;
    }

    /// <summary>Sets a negated consequence that emits an event with the specified category when conditions are false.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact1, TFact2, object>[] parameters)
    {
        Negated = true;
        Consequence = (f1, f2) => _BuildMessage(f1, f2, category, group, template, parameters);
        return this;
    }

    /// <summary>Sets a negated consequence that adds the specified weight to the affirmation score when conditions are false.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> OtherwiseAffirm(int weight)
    {
        Negated = true;
        Consequence = (f1, f2) => HandleAffirm(weight);
        return this;
    }

    /// <summary>Sets a negated consequence that adds the specified weight to the veto score when conditions are false.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> OtherwiseVeto(int weight)
    {
        Negated = true;
        Consequence = (f1, f2) => HandleVeto(weight);
        return this;
    }

    /// <summary>Signals that the consequence modifies facts, triggering re-evaluation.</summary>
    /// <param name="modifyFunc">A function that returns the modified fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2> Modifies(Func<TFact1, TFact2, object> modifyFunc)
    {
        ModifyFunc = modifyFunc;
        return this;
    }

    /// <summary>Inserts a referenced object extracted from the facts into the fact space for forward chaining.</summary>
    /// <typeparam name="TRef">The type of the referenced object.</typeparam>
    /// <param name="extractor">A function that extracts the referenced object from the facts.</param>
    public void Cascade<TRef>(Func<TFact1, TFact2, TRef> extractor) where TRef : class
    {
        Guard.IsNotNull(extractor);
        CascadeAction = (f1, f2) => RuleThreadLocalStorage.CurrentContext.InsertFact(extractor(f1, f2));
    }

    /// <summary>Inserts all items from a collection extracted from the facts into the fact space for forward chaining.</summary>
    /// <typeparam name="TChild">The type of the child items.</typeparam>
    /// <param name="extractor">A function that extracts the child collection from the facts.</param>
    public void CascadeAll<TChild>(Func<TFact1, TFact2, IEnumerable<TChild>> extractor) where TChild : class
    {
        Guard.IsNotNull(extractor);
        CascadeAction = (f1, f2) => _CascadeCollection(extractor(f1, f2));
    }

    private static void _CascadeCollection(IEnumerable<object> children)
    {
        foreach (object o in children)
            RuleThreadLocalStorage.CurrentContext.InsertFact(o);
    }


    private static void _BuildMessage(TFact1 fact1, TFact2 fact2, RuleEvent.EventCategory category, string group, string template, Func<TFact1, TFact2, object>[] parameters)
    {
        if (parameters.Length == 0)
        {
            RuleThreadLocalStorage.CurrentContext.Event(category, group, template);
            return;
        }

        var markers = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            object o = parameters[i](fact1, fact2) ?? "null";
            markers[i] = o;
        }

        string desc = string.Format(System.Globalization.CultureInfo.InvariantCulture, template, markers);
        RuleThreadLocalStorage.CurrentContext.Event(category, group, desc);
    }


    /// <inheritdoc />
    protected override IRule InternalEvaluate(object[] offered)
    {

        if (CascadeAction is not null)
            return this;

        base.InternalEvaluate(offered);

        var fact1 = (TFact1)offered[0];
        var fact2 = (TFact2)offered[1];

        foreach (var cond in Conditions)
        {
            if (cond(fact1, fact2) == Negated)
                return null;
        }

        return this;

    }

    /// <inheritdoc />
    protected override void InternalFire(object[] offered)
    {

        var fact1 = (TFact1)offered[0];
        var fact2 = (TFact2)offered[1];

        if (CascadeAction is not null)
        {
            CascadeAction(fact1, fact2);
            return;
        }

        base.InternalFire(offered);

        Consequence(fact1, fact2);

        if (ModifyFunc is not null)
            DispatchModify(ModifyFunc(fact1, fact2));
    }
}


/// <summary>
/// A concrete rule that evaluates conditions and fires consequences against three fact types.
/// </summary>
/// <typeparam name="TFact1">The first fact type.</typeparam>
/// <typeparam name="TFact2">The second fact type.</typeparam>
/// <typeparam name="TFact3">The third fact type.</typeparam>
public sealed class Rule<TFact1, TFact2, TFact3> : AbstractRule
{
    /// <summary>Initializes a new instance of the <see cref="Rule{TFact1, TFact2, TFact3}"/> class.</summary>
    /// <param name="setName">The namespace (set name) for this rule.</param>
    /// <param name="ruleName">The name of this rule.</param>
    public Rule(string setName, string ruleName)
        : base(setName, ruleName)
    {
        Negated = false;

        Conditions = [];
        Consequence = null;
    }

    /// <summary>Gets a value indicating whether this rule's conditions are negated (fires when conditions are false).</summary>
    public bool Negated { get; private set; }

    private Action<TFact1, TFact2, TFact3> CascadeAction { get; set; }

    private List<Func<TFact1, TFact2, TFact3, bool>> Conditions { get; set; }
    private Action<TFact1, TFact2, TFact3> Consequence { get; set; }
    private Func<TFact1, TFact2, TFact3, object> ModifyFunc { get; set; }

    /// <summary>Sets the salience (priority) for this rule.</summary>
    /// <param name="value">The salience value; higher values evaluate first.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> WithSalience(int value) { Salience = value; return this; }

    /// <summary>Assigns this rule to a mutex group; only one rule per group fires per tuple.</summary>
    /// <param name="name">The mutex group name.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> InMutex(string name) { Mutex = name; return this; }

    /// <summary>Sets the earliest date/time at which this rule becomes active.</summary>
    /// <param name="inception">The inception date/time.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> WithInception(DateTime inception) { Inception = inception; return this; }

    /// <summary>Sets the latest date/time at which this rule remains active.</summary>
    /// <param name="expiration">The expiration date/time.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> WithExpiration(DateTime expiration) { Expiration = expiration; return this; }

    /// <summary>Restricts this rule to fire at most once per evaluation session.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> FireOnce() { OnlyFiresOnce = true; return this; }

    /// <summary>Allows this rule to fire multiple times per evaluation session.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> FireAlways() { OnlyFiresOnce = false; return this; }

    /// <summary>Adds a condition that must be true for this rule to fire.</summary>
    /// <param name="oCondition">The condition predicate.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> If(Func<TFact1, TFact2, TFact3, bool> oCondition) { Conditions.Add(oCondition); return this; }

    /// <summary>Adds an additional condition (AND-joined) that must be true for this rule to fire.</summary>
    /// <param name="oCondition">The condition predicate.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> And(Func<TFact1, TFact2, TFact3, bool> oCondition) { Conditions.Add(oCondition); return this; }

    /// <summary>Sets a no-op consequence; the rule tracks evaluation but performs no action.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> NoConsequence() { Consequence = (f1, f2, f3) => { }; return this; }

    /// <summary>Sets the consequence action to execute when conditions are met.</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Then(Action<TFact1, TFact2, TFact3> oConsequence) { Consequence = oConsequence; return this; }

    /// <summary>Sets the consequence to emit an informational event using the specified message template.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Then(string template, params Func<TFact1, TFact2, TFact3, object>[] parameters) { Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, RuleEvent.EventCategory.Info, "", template, parameters); return this; }

    /// <summary>Sets the consequence to emit an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Then(string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters) { Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, RuleEvent.EventCategory.Info, group, template, parameters); return this; }

    /// <summary>Sets the consequence to emit an event with the specified category, group, and message template.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Then(RuleEvent.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters) { Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, category, group, template, parameters); return this; }

    /// <summary>Sets the consequence to add the specified weight to the affirmation score.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> ThenAffirm(int weight) { Consequence = (f1, f2, f3) => HandleAffirm(weight); return this; }

    /// <summary>Sets the consequence to add the specified weight to the veto score.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> ThenVeto(int weight) { Consequence = (f1, f2, f3) => HandleVeto(weight); return this; }

    /// <summary>Sets a consequence that always fires (no conditions).</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Fire(Action<TFact1, TFact2, TFact3> oConsequence) { Conditions.Add((f1, f2, f3) => true); Consequence = oConsequence; return this; }

    /// <summary>Sets a consequence that always fires, emitting an informational event.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Fire(string template, params Func<TFact1, TFact2, TFact3, object>[] parameters) { Conditions.Add((f1, f2, f3) => true); Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, RuleEvent.EventCategory.Info, "", template, parameters); return this; }

    /// <summary>Sets a consequence that always fires, emitting an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Fire(string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters) { Conditions.Add((f1, f2, f3) => true); Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, RuleEvent.EventCategory.Info, group, template, parameters); return this; }

    /// <summary>Sets a consequence that always fires, emitting an event with the specified category.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Fire(RuleEvent.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters) { Conditions.Add((f1, f2, f3) => true); Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, category, group, template, parameters); return this; }

    /// <summary>Sets a consequence that always fires, adding the specified weight to the affirmation score.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> FireAffirm(int weight) { Conditions.Add((f1, f2, f3) => true); Consequence = (f1, f2, f3) => HandleAffirm(weight); return this; }

    /// <summary>Sets a consequence that always fires, adding the specified weight to the veto score.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> FireVeto(int weight) { Conditions.Add((f1, f2, f3) => true); Consequence = (f1, f2, f3) => HandleVeto(weight); return this; }

    /// <summary>Sets a negated consequence that fires when conditions are false.</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Otherwise(Action<TFact1, TFact2, TFact3> oConsequence) { Negated = true; Consequence = oConsequence; return this; }

    /// <summary>Sets a negated consequence that emits an informational event when conditions are false.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Otherwise(string template, params Func<TFact1, TFact2, TFact3, object>[] parameters) { Negated = true; Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, RuleEvent.EventCategory.Info, "", template, parameters); return this; }

    /// <summary>Sets a negated consequence that emits an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Otherwise(string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters) { Negated = true; Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, RuleEvent.EventCategory.Info, group, template, parameters); return this; }

    /// <summary>Sets a negated consequence that emits an event with the specified category when conditions are false.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters) { Negated = true; Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, category, group, template, parameters); return this; }

    /// <summary>Sets a negated consequence that adds the specified weight to the affirmation score when conditions are false.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> OtherwiseAffirm(int weight) { Negated = true; Consequence = (f1, f2, f3) => HandleAffirm(weight); return this; }

    /// <summary>Sets a negated consequence that adds the specified weight to the veto score when conditions are false.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> OtherwiseVeto(int weight) { Negated = true; Consequence = (f1, f2, f3) => HandleVeto(weight); return this; }

    /// <summary>Signals that the consequence modifies facts, triggering re-evaluation.</summary>
    /// <param name="modifyFunc">A function that returns the modified fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3> Modifies(Func<TFact1, TFact2, TFact3, object> modifyFunc) { ModifyFunc = modifyFunc; return this; }

    /// <summary>Inserts a referenced object extracted from the facts into the fact space for forward chaining.</summary>
    /// <typeparam name="TRef">The type of the referenced object.</typeparam>
    /// <param name="extractor">A function that extracts the referenced object from the facts.</param>
    public void Cascade<TRef>(Func<TFact1, TFact2, TFact3, TRef> extractor) where TRef : class { Guard.IsNotNull(extractor); CascadeAction = (f1, f2, f3) => RuleThreadLocalStorage.CurrentContext.InsertFact(extractor(f1, f2, f3)); }

    /// <summary>Inserts all items from a collection extracted from the facts into the fact space for forward chaining.</summary>
    /// <typeparam name="TChild">The type of the child items.</typeparam>
    /// <param name="extractor">A function that extracts the child collection from the facts.</param>
    public void CascadeAll<TChild>(Func<TFact1, TFact2, TFact3, IEnumerable<TChild>> extractor) where TChild : class { Guard.IsNotNull(extractor); CascadeAction = (f1, f2, f3) => _CascadeCollection(extractor(f1, f2, f3)); }

    private static void _CascadeCollection(IEnumerable<object> children)
    {
        foreach (object o in children)
            RuleThreadLocalStorage.CurrentContext.InsertFact(o);
    }


    private static void _BuildMessage(TFact1 fact1, TFact2 fact2, TFact3 fact3, RuleEvent.EventCategory category, string group, string template, Func<TFact1, TFact2, TFact3, object>[] parameters)
    {
        if (parameters.Length == 0)
        {
            RuleThreadLocalStorage.CurrentContext.Event(category, group, template);
            return;
        }

        var markers = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            object o = parameters[i](fact1, fact2, fact3) ?? "null";
            markers[i] = o;
        }

        string desc = string.Format(System.Globalization.CultureInfo.InvariantCulture, template, markers);
        RuleThreadLocalStorage.CurrentContext.Event(category, group, desc);
    }


    /// <inheritdoc />
    protected override IRule InternalEvaluate(object[] offered)
    {

        if (CascadeAction is not null)
            return this;

        base.InternalEvaluate(offered);

        var fact1 = (TFact1)offered[0];
        var fact2 = (TFact2)offered[1];
        var fact3 = (TFact3)offered[2];

        foreach (var cond in Conditions)
        {
            if (cond(fact1, fact2, fact3) == Negated)
                return null;
        }

        return this;

    }

    /// <inheritdoc />
    protected override void InternalFire(object[] offered)
    {

        var fact1 = (TFact1)offered[0];
        var fact2 = (TFact2)offered[1];
        var fact3 = (TFact3)offered[2];

        if (CascadeAction is not null)
        {
            CascadeAction(fact1, fact2, fact3);
            return;
        }

        base.InternalFire(offered);

        Consequence(fact1, fact2, fact3);

        if (ModifyFunc is not null)
            DispatchModify(ModifyFunc(fact1, fact2, fact3));
    }
}


/// <summary>
/// A concrete rule that evaluates conditions and fires consequences against four fact types.
/// </summary>
/// <typeparam name="TFact1">The first fact type.</typeparam>
/// <typeparam name="TFact2">The second fact type.</typeparam>
/// <typeparam name="TFact3">The third fact type.</typeparam>
/// <typeparam name="TFact4">The fourth fact type.</typeparam>
public sealed class Rule<TFact1, TFact2, TFact3, TFact4> : AbstractRule
{
    /// <summary>Initializes a new instance of the <see cref="Rule{TFact1, TFact2, TFact3, TFact4}"/> class.</summary>
    /// <param name="setName">The namespace (set name) for this rule.</param>
    /// <param name="ruleName">The name of this rule.</param>
    public Rule(string setName, string ruleName) : base(setName, ruleName)
    {
        Negated = false;

        Conditions = [];
        Consequence = null;
    }

    /// <summary>Gets a value indicating whether this rule's conditions are negated (fires when conditions are false).</summary>
    public bool Negated { get; private set; }

    private Action<TFact1, TFact2, TFact3, TFact4> CascadeAction { get; set; }

    private List<Func<TFact1, TFact2, TFact3, TFact4, bool>> Conditions { get; set; }
    private Action<TFact1, TFact2, TFact3, TFact4> Consequence { get; set; }
    private Func<TFact1, TFact2, TFact3, TFact4, object> ModifyFunc { get; set; }


    /// <summary>Sets the salience (priority) for this rule.</summary>
    /// <param name="value">The salience value; higher values evaluate first.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> WithSalience(int value) { Salience = value; return this; }

    /// <summary>Assigns this rule to a mutex group; only one rule per group fires per tuple.</summary>
    /// <param name="name">The mutex group name.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> InMutex(string name) { Mutex = name; return this; }

    /// <summary>Sets the earliest date/time at which this rule becomes active.</summary>
    /// <param name="inception">The inception date/time.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> WithInception(DateTime inception) { Inception = inception; return this; }

    /// <summary>Sets the latest date/time at which this rule remains active.</summary>
    /// <param name="expiration">The expiration date/time.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> WithExpiration(DateTime expiration) { Expiration = expiration; return this; }

    /// <summary>Restricts this rule to fire at most once per evaluation session.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> FireOnce() { OnlyFiresOnce = true; return this; }

    /// <summary>Allows this rule to fire multiple times per evaluation session.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> FireAlways() { OnlyFiresOnce = false; return this; }

    /// <summary>Adds a condition that must be true for this rule to fire.</summary>
    /// <param name="oCondition">The condition predicate.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> If(Func<TFact1, TFact2, TFact3, TFact4, bool> oCondition) { Conditions.Add(oCondition); return this; }

    /// <summary>Adds an additional condition (AND-joined) that must be true for this rule to fire.</summary>
    /// <param name="oCondition">The condition predicate.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> And(Func<TFact1, TFact2, TFact3, TFact4, bool> oCondition) { Conditions.Add(oCondition); return this; }

    /// <summary>Sets a no-op consequence; the rule tracks evaluation but performs no action.</summary>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> NoConsequence() { Consequence = (f1, f2, f3, f4) => { }; return this; }

    /// <summary>Sets the consequence action to execute when conditions are met.</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Then(Action<TFact1, TFact2, TFact3, TFact4> oConsequence) { Consequence = oConsequence; return this; }

    /// <summary>Sets the consequence to emit an informational event using the specified message template.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Then(string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters) { Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, RuleEvent.EventCategory.Info, "", template, parameters); return this; }

    /// <summary>Sets the consequence to emit an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Then(string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters) { Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, RuleEvent.EventCategory.Info, group, template, parameters); return this; }

    /// <summary>Sets the consequence to emit an event with the specified category, group, and message template.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Then(RuleEvent.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters) { Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, category, group, template, parameters); return this; }

    /// <summary>Sets the consequence to add the specified weight to the affirmation score.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> ThenAffirm(int weight) { Consequence = (f1, f2, f3, f4) => HandleAffirm(weight); return this; }

    /// <summary>Sets the consequence to add the specified weight to the veto score.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> ThenVeto(int weight) { Consequence = (f1, f2, f3, f4) => HandleVeto(weight); return this; }

    /// <summary>Sets a consequence that always fires (no conditions).</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Fire(Action<TFact1, TFact2, TFact3, TFact4> oConsequence) { Conditions.Add((f1, f2, f3, f4) => true); Consequence = oConsequence; return this; }

    /// <summary>Sets a consequence that always fires, emitting an informational event.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Fire(string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters) { Conditions.Add((f1, f2, f3, f4) => true); Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, RuleEvent.EventCategory.Info, "", template, parameters); return this; }

    /// <summary>Sets a consequence that always fires, emitting an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Fire(string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters) { Conditions.Add((f1, f2, f3, f4) => true); Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, RuleEvent.EventCategory.Info, group, template, parameters); return this; }

    /// <summary>Sets a consequence that always fires, emitting an event with the specified category.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Fire(RuleEvent.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters) { Conditions.Add((f1, f2, f3, f4) => true); Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, category, group, template, parameters); return this; }

    /// <summary>Sets a consequence that always fires, adding the specified weight to the affirmation score.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> FireAffirm(int weight) { Conditions.Add((f1, f2, f3, f4) => true); Consequence = (f1, f2, f3, f4) => HandleAffirm(weight); return this; }

    /// <summary>Sets a consequence that always fires, adding the specified weight to the veto score.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> FireVeto(int weight) { Conditions.Add((f1, f2, f3, f4) => true); Consequence = (f1, f2, f3, f4) => HandleVeto(weight); return this; }

    /// <summary>Sets a negated consequence that fires when conditions are false.</summary>
    /// <param name="oConsequence">The action to execute.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Otherwise(Action<TFact1, TFact2, TFact3, TFact4> oConsequence) { Negated = true; Consequence = oConsequence; return this; }

    /// <summary>Sets a negated consequence that emits an informational event when conditions are false.</summary>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Otherwise(string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters) { Negated = true; Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, RuleEvent.EventCategory.Info, "", template, parameters); return this; }

    /// <summary>Sets a negated consequence that emits an informational event in the specified group.</summary>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Otherwise(string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters) { Negated = true; Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, RuleEvent.EventCategory.Info, group, template, parameters); return this; }

    /// <summary>Sets a negated consequence that emits an event with the specified category when conditions are false.</summary>
    /// <param name="category">The event category.</param>
    /// <param name="group">The event group.</param>
    /// <param name="template">The message template.</param>
    /// <param name="parameters">Functions that extract parameter values from the facts.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters) { Negated = true; Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, category, group, template, parameters); return this; }

    /// <summary>Sets a negated consequence that adds the specified weight to the affirmation score when conditions are false.</summary>
    /// <param name="weight">The affirmation weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> OtherwiseAffirm(int weight) { Negated = true; Consequence = (f1, f2, f3, f4) => HandleAffirm(weight); return this; }

    /// <summary>Sets a negated consequence that adds the specified weight to the veto score when conditions are false.</summary>
    /// <param name="weight">The veto weight.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> OtherwiseVeto(int weight) { Negated = true; Consequence = (f1, f2, f3, f4) => HandleVeto(weight); return this; }

    /// <summary>Signals that the consequence modifies facts, triggering re-evaluation.</summary>
    /// <param name="modifyFunc">A function that returns the modified fact.</param>
    /// <returns>This rule for fluent chaining.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> Modifies(Func<TFact1, TFact2, TFact3, TFact4, object> modifyFunc) { ModifyFunc = modifyFunc; return this; }

    /// <summary>Inserts a referenced object extracted from the facts into the fact space for forward chaining.</summary>
    /// <typeparam name="TRef">The type of the referenced object.</typeparam>
    /// <param name="extractor">A function that extracts the referenced object from the facts.</param>
    public void Cascade<TRef>(Func<TFact1, TFact2, TFact3, TFact4, TRef> extractor) where TRef : class { Guard.IsNotNull(extractor); CascadeAction = (f1, f2, f3, f4) => RuleThreadLocalStorage.CurrentContext.InsertFact(extractor(f1, f2, f3, f4)); }

    /// <summary>Inserts all items from a collection extracted from the facts into the fact space for forward chaining.</summary>
    /// <typeparam name="TChild">The type of the child items.</typeparam>
    /// <param name="extractor">A function that extracts the child collection from the facts.</param>
    public void CascadeAll<TChild>(Func<TFact1, TFact2, TFact3, TFact4, IEnumerable<TChild>> extractor) where TChild : class { Guard.IsNotNull(extractor); CascadeAction = (f1, f2, f3, f4) => _CascadeCollection(extractor(f1, f2, f3, f4)); }

    private static void _CascadeCollection(IEnumerable<object> children)
    {
        foreach (object o in children)
            RuleThreadLocalStorage.CurrentContext.InsertFact(o);
    }


    private static void _BuildMessage(TFact1 fact1, TFact2 fact2, TFact3 fact3, TFact4 fact4, RuleEvent.EventCategory category, string group, string template, Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters)
    {
        if (parameters.Length == 0)
        {
            RuleThreadLocalStorage.CurrentContext.Event(category, group, template);
            return;
        }

        var markers = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            object o = parameters[i](fact1, fact2, fact3, fact4) ?? "null";
            markers[i] = o;
        }

        string desc = string.Format(System.Globalization.CultureInfo.InvariantCulture, template, markers);
        RuleThreadLocalStorage.CurrentContext.Event(category, group, desc);
    }


    /// <inheritdoc />
    protected override IRule InternalEvaluate(object[] offered)
    {

        if (CascadeAction is not null)
            return this;

        base.InternalEvaluate(offered);

        var fact1 = (TFact1)offered[0];
        var fact2 = (TFact2)offered[1];
        var fact3 = (TFact3)offered[2];
        var fact4 = (TFact4)offered[3];

        foreach (var cond in Conditions)
        {
            if (cond(fact1, fact2, fact3, fact4) == Negated)
                return null;
        }

        return this;

    }

    /// <inheritdoc />
    protected override void InternalFire(object[] offered)
    {

        var fact1 = (TFact1)offered[0];
        var fact2 = (TFact2)offered[1];
        var fact3 = (TFact3)offered[2];
        var fact4 = (TFact4)offered[3];

        if (CascadeAction is not null)
        {
            CascadeAction(fact1, fact2, fact3, fact4);
            return;
        }

        base.InternalFire(offered);

        Consequence(fact1, fact2, fact3, fact4);

        if (ModifyFunc is not null)
            DispatchModify(ModifyFunc(fact1, fact2, fact3, fact4));
    }
}
