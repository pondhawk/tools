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

using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Evaluation;

namespace Pondhawk.Rules;

/// <summary>
/// A rule that iterates over child facts extracted from a parent fact, evaluating conditions and firing consequences per child.
/// </summary>
/// <remarks>
/// <para>Child facts are <b>not</b> inserted into the fact space and do not participate in forward chaining.
/// Only the parent fact is tracked. Use <c>Modifies()</c> to signal that the parent was modified,
/// which triggers re-evaluation. If children must participate in forward chaining, use <c>Cascade</c> instead.</para>
/// <para>The extractor function (e.g. <c>p =&gt; p.Children</c>) is called during evaluation to get the collection.
/// Each child that passes all conditions has its consequence fired.</para>
/// </remarks>
public sealed class ForeachRule<TParent, TFact> : AbstractRule
{

    public ForeachRule(Func<TParent, IEnumerable<TFact>> extractor, string setName, string ruleName) : base(setName, ruleName)
    {

        Extractor = extractor;

        Conditions = [];
        Consequence = null;

    }

    private Func<TParent, IEnumerable<TFact>> Extractor { get; }

    public bool Negated { get; private set; }

    private List<Func<TFact, bool>> Conditions { get; set; }
    private Action<TFact> Consequence { get; set; }
    private Func<TParent, object> ModifyFunc { get; set; }


    public ForeachRule<TParent, TFact> WithSalience(int value)
    {
        Salience = value;
        return this;
    }


    public ForeachRule<TParent, TFact> InMutex(string name)
    {
        Mutex = name;
        return this;
    }



    public ForeachRule<TParent, TFact> WithInception(DateTime inception)
    {
        Inception = inception;
        return this;
    }


    public ForeachRule<TParent, TFact> WithExpiration(DateTime expiration)
    {
        Expiration = expiration;
        return this;
    }


    public ForeachRule<TParent, TFact> FireOnce()
    {
        OnlyFiresOnce = true;
        return this;
    }


    public ForeachRule<TParent, TFact> FireAlways()
    {
        OnlyFiresOnce = false;
        return this;
    }



    public ForeachRule<TParent, TFact> If(Func<TFact, bool> oCondition)
    {
        Conditions.Add(oCondition);
        return this;
    }


    public ForeachRule<TParent, TFact> And(Func<TFact, bool> oCondition)
    {
        Conditions.Add(oCondition);
        return this;
    }



    public ForeachRule<TParent, TFact> Then(Action<TFact> oConsequence)
    {
        Consequence = oConsequence;
        return this;
    }


    public ForeachRule<TParent, TFact> Then(string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, "", template, parameters);
        return this;
    }


    public ForeachRule<TParent, TFact> Then(string group, string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, group, template, parameters);
        return this;
    }



    public ForeachRule<TParent, TFact> Then(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, category, group, template, parameters);
        return this;
    }


    public ForeachRule<TParent, TFact> ThenAffirm(int weight)
    {
        Consequence = f => HandleAffirm(weight);
        return this;
    }


    public ForeachRule<TParent, TFact> ThenVeto(int weight)
    {
        Consequence = s => HandleVeto(weight);
        return this;
    }



    public ForeachRule<TParent, TFact> Fire(Action<TFact> oConsequence)
    {
        Conditions.Add(f => true);
        Consequence = oConsequence;
        return this;
    }


    public ForeachRule<TParent, TFact> Fire(string template, params Func<TFact, object>[] parameters)
    {
        Conditions.Add(f => true);
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, "", template, parameters);
        return this;
    }


    public ForeachRule<TParent, TFact> Fire(string group, string template, params Func<TFact, object>[] parameters)
    {
        Conditions.Add(f => true);
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, group, template, parameters);
        return this;
    }


    public ForeachRule<TParent, TFact> Fire(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters)
    {
        Conditions.Add(f => true);
        Consequence = f => _BuildMessage(f, category, group, template, parameters);
        return this;
    }


    public ForeachRule<TParent, TFact> FireAffirm(int weight)
    {
        Conditions.Add(f => true);
        Consequence = f => HandleAffirm(weight);
        return this;
    }


    public ForeachRule<TParent, TFact> FireVeto(int weight)
    {
        Conditions.Add(f => true);
        Consequence = f => HandleVeto(weight);
        return this;
    }



    public ForeachRule<TParent, TFact> Otherwise(Action<TFact> oConsequence)
    {
        Negated = true;
        Consequence = oConsequence;
        return this;
    }


    public ForeachRule<TParent, TFact> Otherwise(string template, params Func<TFact, object>[] parameters)
    {
        Negated = true;
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, "", template, parameters);
        return this;
    }


    public ForeachRule<TParent, TFact> Otherwise(string group, string template, params Func<TFact, object>[] parameters)
    {
        Negated = true;
        Consequence = f => _BuildMessage(f, RuleEvent.EventCategory.Info, group, template, parameters);
        return this;
    }


    public ForeachRule<TParent, TFact> Otherwise(RuleEvent.EventCategory category, string group, string template, params Func<TFact, object>[] parameters)
    {
        Negated = true;
        Consequence = f => _BuildMessage(f, category, group, template, parameters);
        return this;
    }


    public ForeachRule<TParent, TFact> OtherwiseAffirm(int weight)
    {
        Negated = true;
        Consequence = f => HandleAffirm(weight);
        return this;
    }


    public ForeachRule<TParent, TFact> OtherwiseVeto(int weight)
    {
        Negated = true;
        Consequence = f => HandleVeto(weight);
        return this;
    }


    public ForeachRule<TParent, TFact> Modifies()
    {
        ModifyFunc = f => f;
        return this;
    }

    public ForeachRule<TParent, TFact> Modifies(Func<TParent, object> modifyFunc)
    {
        ModifyFunc = modifyFunc;
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
            object o = parameters[i](fact) ?? "null";
            markers[i] = o;
        }

        string desc = string.Format(System.Globalization.CultureInfo.InvariantCulture, template, markers);
        RuleThreadLocalStorage.CurrentContext.Event(category, group, desc, fact);
    }


    protected override IRule InternalEvaluate(object[] offered)
    {

        base.InternalEvaluate(offered);

        var parent = (TParent)offered[0];

        List<TFact> trueFacts = [];

        foreach (TFact fact in Extractor(parent))
        {

            foreach (var cond in Conditions)
            {
                if (cond(fact) == Negated)
                    break;

                trueFacts.Add(fact);
            }

        }


        if (trueFacts.Count > 0)
        {
            var sub = new SubRule<TParent, TFact>(trueFacts, Consequence, parent, ModifyFunc)
            {
                Namespace = Namespace,
                Name = Name,
                Inception = Inception,
                Expiration = Expiration,
                Mutex = Mutex,
                OnlyFiresOnce = OnlyFiresOnce,
                Salience = Salience
            };

            return sub;
        }

        return null;

    }


    protected override void InternalFire(object[] offered)
    {

    }


}

