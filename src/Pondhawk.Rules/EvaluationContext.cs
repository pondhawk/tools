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
using Pondhawk.Rules.Evaluation;
using Pondhawk.Rules.Listeners;

namespace Pondhawk.Rules;

/// <summary>
/// Holds the state for a single rule evaluation session, including facts, results, configuration, and lookup tables.
/// </summary>
/// <remarks>
/// <para>By default, evaluation throws <see cref="Exceptions.ViolationsExistException"/> if violations are found
/// and <see cref="Exceptions.NoRulesEvaluatedException"/> if no rules match. Use <c>SuppressExceptions()</c>,
/// <c>SuppressValidationException()</c>, or <c>SuppressNoRulesException()</c> to disable.</para>
/// <para><b>Safety limits:</b> <c>MaxEvaluations</c> (default 500,000) and <c>MaxDuration</c> (default 10s)
/// prevent runaway forward-chaining loops. When exceeded, <see cref="Exceptions.EvaluationExhaustedException"/> is thrown.</para>
/// <para><b>Lookup tables:</b> <c>AddLookup</c> registers keyed data that rules can access via <c>Lookup&lt;T&gt;(key)</c>
/// during consequence execution, enabling join-like lookups without inserting reference data as facts.</para>
/// <para><b>Forward chaining:</b> <c>InsertFact</c>/<c>ModifyFact</c>/<c>RetractFact</c> are called from rule consequences
/// to trigger re-evaluation of affected rules in the current session.</para>
/// <para><b>Shared state:</b> The <c>Shared</c> dictionary allows rules to pass data between consequences.</para>
/// </remarks>
public sealed class EvaluationContext
{
    /// <summary>Initializes a new instance of the <see cref="EvaluationContext"/> class with default configuration.</summary>
    public EvaluationContext()
    {
        ThrowValidationException = true;
        ThrowNoRulesException = true;

        Listener = new NoopEvaluationListener();

        Space = new();

        Tables = new(StringComparer.Ordinal);

        Results = new();

        CurrentRuleName = "";
        ModificationsOccurred = false;
        InsertionsOccurred = false;

        Description = "";

        MaxEvaluations = 500000;
        MaxDuration = 10_000;

        MaxViolations = int.MaxValue;

        Mutexed = [];
        FireOnceRules = new();

    }


    internal FactSpace Space { get; }

    internal string CurrentRuleName { get; set; }

    internal long CurrentIdentity { get; set; }
    internal long CurrentSelector { get; set; }
    internal object[] CurrentTuple { get; set; }

    internal readonly int[] SelectorBuffer = new int[4];
    internal readonly int[] IdentityBuffer = new int[4];
    internal readonly object[][] TupleBuffers = [new object[1], new object[2], new object[3], new object[4]];
    internal int CurrentArity { get; set; }


    internal bool ModificationsOccurred { get; private set; }
    internal bool InsertionsOccurred { get; private set; }

    internal HashSet<string> Mutexed { get; }
    internal Dictionary<object, ISet<long>> FireOnceRules { get; set; }


    internal long StartedTick { get; set; } = Environment.TickCount64;

    internal bool IsExhausted => (Results.TotalEvaluated > MaxEvaluations) || ((Environment.TickCount64 - StartedTick) > MaxDuration);


    /// <summary>Gets or sets an optional description for this evaluation session.</summary>
    public string Description { get; set; }

    /// <summary>Gets or sets a value indicating whether to throw <see cref="Exceptions.ViolationsExistException"/> when violations are found.</summary>
    public bool ThrowValidationException { get; set; }

    /// <summary>Gets or sets a value indicating whether to throw <see cref="Exceptions.NoRulesEvaluatedException"/> when no rules match.</summary>
    public bool ThrowNoRulesException { get; set; }

    /// <summary>Gets or sets the evaluation listener for tracing rule evaluation.</summary>
    public IEvaluationListener Listener { get; set; }

    /// <summary>Gets the accumulated evaluation results for this session.</summary>
    public EvaluationResults Results { get; }

    /// <summary>Gets or sets the maximum number of evaluations before exhaustion (default 500,000).</summary>
    public int MaxEvaluations { get; set; }

    /// <summary>Gets or sets the maximum duration in milliseconds before exhaustion (default 10,000).</summary>
    public long MaxDuration { get; set; }

    /// <summary>Gets or sets the maximum number of violations before evaluation stops early.</summary>
    public int MaxViolations { get; set; }

    internal bool ViolationsExceeded => Results.ViolationCount >= MaxViolations;



    private Dictionary<string, IDictionary<object, object>> Tables { get; }

    /// <summary>Registers a lookup table keyed by a member extractor, using the member type name as the table name.</summary>
    /// <typeparam name="TMember">The type of items in the lookup table.</typeparam>
    /// <param name="keyExtractor">A function that extracts the lookup key from each member.</param>
    /// <param name="members">The members to add to the lookup table.</param>
    public void AddLookup<TMember>(Func<TMember, object> keyExtractor, IEnumerable<TMember> members)
    {
        var name = typeof(TMember).FullName;
        AddLookup(name, keyExtractor, members);
    }

    /// <summary>Registers a named lookup table keyed by a member extractor.</summary>
    /// <typeparam name="TMember">The type of items in the lookup table.</typeparam>
    /// <param name="name">The name for the lookup table.</param>
    /// <param name="keyExtractor">A function that extracts the lookup key from each member.</param>
    /// <param name="members">The members to add to the lookup table.</param>
    public void AddLookup<TMember>(string name, Func<TMember, object> keyExtractor, IEnumerable<TMember> members)
    {

        Dictionary<object, object> table = new();

        foreach (var m in members)
        {
            object key = keyExtractor(m);
            table[key] = m;
        }

        Tables[name] = table;

    }

    /// <summary>Registers a pre-built named lookup table.</summary>
    /// <param name="name">The name for the lookup table.</param>
    /// <param name="table">The dictionary to use as the lookup table.</param>
    public void AddLookup(string name, IDictionary<object, object> table)
    {
        Tables[name] = table;
    }

    /// <summary>Looks up a member by key from the default lookup table for the member type.</summary>
    /// <typeparam name="TMember">The type of the member to retrieve.</typeparam>
    /// <param name="key">The key to look up.</param>
    /// <returns>The member associated with the key.</returns>
    public TMember Lookup<TMember>(object key)
    {
        var name = typeof(TMember).FullName;
        return Lookup<TMember>(name, key);
    }

    /// <summary>Looks up a member by key from a named lookup table.</summary>
    /// <typeparam name="TMember">The type of the member to retrieve.</typeparam>
    /// <param name="name">The name of the lookup table.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The member associated with the key.</returns>
    public TMember Lookup<TMember>(string name, object key)
    {
        if (!Tables.TryGetValue(name, out var table))
            throw new InvalidOperationException($"Could not find lookup table with the name {name}");

        if (!table.TryGetValue(key, out var member))
            throw new InvalidOperationException($"Could not find member using key {key} from table {name}");

        if (member is not TMember)
            throw new InvalidOperationException($"Could not cast member to type {typeof(TMember).FullName} using key {key} from table {name}");

        return (TMember)member;

    }



    /// <summary>Gets the shared dictionary for passing data between rule consequences.</summary>
    public IDictionary<string, object> Shared => Results.Shared;

    internal void InsertFact(object fact)
    {
        Guard.IsNotNull(fact);

        int index = _SelectorFromFact(fact);
        if (index == 0)
        {
            Space.InsertFact(fact);
            InsertionsOccurred = true;
        }
    }


    internal void ModifyFact(object fact)
    {
        Guard.IsNotNull(fact);

        int index = _SelectorFromFact(fact);
        if (index > 0)
        {
            Space.ModifyFact(index);
            ModificationsOccurred = true;
        }
    }


    internal void RetractFact(object fact)
    {
        Guard.IsNotNull(fact);

        int index = _SelectorFromFact(fact);
        if (index > 0)
        {
            Space.RetractFact(index);
            ModificationsOccurred = true;
        }
    }

    private int _SelectorFromFact(object fact)
    {
        for (var i = 0; i < CurrentArity; i++)
            if (fact == CurrentTuple[i])
                return SelectorBuffer[i];

        return 0;
    }

    internal void ResetBetweenTuples()
    {
        InsertionsOccurred = false;
    }


    internal void ResetBetweenRules()
    {
        CurrentRuleName = "";
        ModificationsOccurred = false;
    }


    internal void Event(RuleEvent.EventCategory category, string group, string template, params object[] parameters)
    {


        Guard.IsNotNullOrWhiteSpace(template);

        var message = parameters.Length == 0 ? template : string.Format(System.Globalization.CultureInfo.InvariantCulture, template, parameters);

        var theEvent = new RuleEvent
        {
            Category = category,
            Group = group,
            RuleName = CurrentRuleName,
            MessageTemplate = template,
            Message = message
        };

        Results.Events.Add(theEvent);
        if (category == RuleEvent.EventCategory.Violation)
            Results.ViolationCount++;

    }


    /// <summary>Adds one or more facts to the fact space for evaluation.</summary>
    /// <param name="facts">The facts to add.</param>
    public void AddFacts(params object[] facts)
    {
        Space.Add(facts);
    }

    /// <summary>Adds all facts from the collection to the fact space for evaluation.</summary>
    /// <param name="facts">The facts to add.</param>
    public void AddAllFacts(IEnumerable<object> facts)
    {
        Space.AddAll(facts);
    }


    // ===== Fluent configuration API =====

    /// <summary>Sets the maximum number of evaluations before exhaustion.</summary>
    /// <param name="max">The maximum evaluation count.</param>
    /// <returns>This context for fluent chaining.</returns>
    public EvaluationContext WithMaxEvaluations(int max) { MaxEvaluations = max; return this; }

    /// <summary>Sets the maximum duration in milliseconds before exhaustion.</summary>
    /// <param name="milliseconds">The maximum duration.</param>
    /// <returns>This context for fluent chaining.</returns>
    public EvaluationContext WithMaxDuration(long milliseconds) { MaxDuration = milliseconds; return this; }

    /// <summary>Sets the maximum number of violations before evaluation stops early.</summary>
    /// <param name="max">The maximum violation count.</param>
    /// <returns>This context for fluent chaining.</returns>
    public EvaluationContext WithMaxViolations(int max) { MaxViolations = max; return this; }

    /// <summary>Sets a description for this evaluation session.</summary>
    /// <param name="description">The description text.</param>
    /// <returns>This context for fluent chaining.</returns>
    public EvaluationContext WithDescription(string description) { Description = description; return this; }

    /// <summary>Sets the evaluation listener for tracing rule evaluation.</summary>
    /// <param name="listener">The listener to use.</param>
    /// <returns>This context for fluent chaining.</returns>
    public EvaluationContext WithListener(IEvaluationListener listener) { Listener = listener; return this; }

    /// <summary>Suppresses both validation and no-rules exceptions.</summary>
    /// <returns>This context for fluent chaining.</returns>
    public EvaluationContext SuppressExceptions() { ThrowValidationException = false; ThrowNoRulesException = false; return this; }

    /// <summary>Suppresses the validation exception thrown when violations are found.</summary>
    /// <returns>This context for fluent chaining.</returns>
    public EvaluationContext SuppressValidationException() { ThrowValidationException = false; return this; }

    /// <summary>Suppresses the exception thrown when no rules match.</summary>
    /// <returns>This context for fluent chaining.</returns>
    public EvaluationContext SuppressNoRulesException() { ThrowNoRulesException = false; return this; }


}
