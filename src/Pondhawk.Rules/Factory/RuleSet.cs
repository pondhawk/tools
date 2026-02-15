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
using Pondhawk.Rules.Tree;

namespace Pondhawk.Rules.Factory;

/// <summary>
/// A runtime-configurable rule set that allows programmatic addition of rules, validations, and foreach rules.
/// </summary>
/// <remarks>
/// Use this when you need to define rules programmatically at runtime rather than in static builder classes.
/// Call <see cref="AddRule{TFact}"/> to add rules, <see cref="AddValidation{TFact}"/> for validations,
/// and <c>Evaluate(facts)</c> to run. Also supports <c>TryValidate</c> and <c>Decide</c> via <see cref="IRuleSet"/>.
/// </remarks>
/// <example>
/// <code>
/// var ruleSet = new RuleSet();
///
/// ruleSet.AddValidation&lt;Person&gt;("name-required")
///     .Assert&lt;string&gt;(p =&gt; p.Name).Required();
///
/// ruleSet.AddRule&lt;Person&gt;("age-check")
///     .When(p =&gt; p.Age &gt;= 18)
///     .Then(p =&gt; p.Status = "Adult");
///
/// var result = ruleSet.Evaluate(new Person { Name = "Alice", Age = 25 });
/// </code>
/// </example>
public sealed class RuleSet : AbstractRuleSet
{
    private readonly HashSet<string> _namespaces = [];
    private readonly RuleTree _tree = new();

    /// <summary>Gets the underlying rule base for this rule set.</summary>
    public IRuleBase RuleBase => _tree;

    /// <summary>Seals the rule tree and builds type-match caches.</summary>
    public void Build()
    {
        _tree.Build();
    }

    /// <summary>Adds all rules from the specified builder to this rule set.</summary>
    /// <param name="builder">The builder whose rules to add.</param>
    public void Add(IBuilder builder)
    {
        builder.LoadRules(_tree);
    }


    /// <inheritdoc />
    protected override IRuleBase GetRuleBase()
    {
        return _tree;
    }

    /// <inheritdoc />
    protected override IEnumerable<string> GetNamespaces()
    {
        return _namespaces;
    }



    /// <summary>Adds a single-fact rule with the specified name.</summary>
    /// <typeparam name="TFact">The fact type the rule operates on.</typeparam>
    /// <param name="name">The name of the rule.</param>
    /// <returns>The newly created rule for fluent configuration.</returns>
    public Rule<TFact> AddRule<TFact>(string name) where TFact : class
    {
        var rule = new Rule<TFact>("runtime", name);
        _tree.Add([typeof(TFact)], [rule]);
        return rule;
    }



    /// <inheritdoc />
    public override EvaluationContext GetEvaluationContext()
    {
        return new();
    }


    /// <summary>
    /// Adds a rule that reasons over the an enumeration of child facts associated with
    /// the type defined for this builder.
    /// </summary>
    /// <remarks>
    /// These child facts are not inserted into the fact space and thus do not trigger
    /// forward chaining if they are modified. However the parent can signal
    /// modification and trigger forward chaining. If it is required that the children
    /// participate in forward chainging use Cascade instead.
    /// </remarks>
    /// <typeparam name="TFact">The parent fact that contains the children that will
    /// actually be evaluated. Also scheduling and order are alos drive by this type as
    /// opposed to the children</typeparam>
    /// <typeparam name="TChild">The type the conditions and consequence are targeting.
    /// Each rule that produces a true evaluation will have its consequence
    /// fired.</typeparam>
    /// <param name="ruleName">The name for the rule. This is required and should be
    /// unique within the builder where the rule is defined. I can be anything you like
    /// and serves no operational function. However it is very useful when you are
    /// troubleshoot (logging) your rules and is used in the EvaluationResults
    /// statistics.</param>
    /// <param name="extractor">The extractor used to access the collection from the
    /// parent fact. The rules are defined for these child facts. The conditions are
    /// evaulated for each child and those that produce a true condition are
    /// fired.</param>
    /// <returns>
    /// The newly created rule for the single given fact type of this builder
    /// </returns>
    /// <example>
    /// var rule = AddRule&lt;Family&gt;( "Gabby Foreach rule", p=&gt;p.Chilldren ).Modifies()
    ///     When( c=&gt;c.Name == "Gabby" ).And( c=&gt;c.Age == 4 )
    ///     Then( c=&gt;c.Status = "Not A baby anymore" )
    /// </example>

    public ForeachRule<TFact, TChild> AddRule<TFact, TChild>(string ruleName, Func<TFact, IEnumerable<TChild>> extractor) where TFact : class where TChild : class
    {

        Guard.IsNotNullOrWhiteSpace(ruleName);
        Guard.IsNotNull(extractor);

        var rule = new ForeachRule<TFact, TChild>(extractor, "runtime", ruleName);

        _tree.Add([typeof(TFact)], [rule]);

        return rule;

    }




    /// <summary>Adds a validation rule for the specified fact type.</summary>
    /// <typeparam name="TFact">The fact type to validate.</typeparam>
    /// <param name="name">The name of the validation rule.</param>
    /// <returns>The newly created validation rule for fluent configuration.</returns>
    public ValidationRule<TFact> AddValidation<TFact>(string name) where TFact : class
    {
        Guard.IsNotNullOrEmpty(name);

        var rule = new ValidationRule<TFact>("runtime", name);
        _tree.Add([typeof(TFact)], [rule]);

        return rule;
    }



    /// <summary>Adds a two-fact rule with the specified name.</summary>
    /// <typeparam name="TFact1">The first fact type.</typeparam>
    /// <typeparam name="TFact2">The second fact type.</typeparam>
    /// <param name="name">The name of the rule.</param>
    /// <returns>The newly created rule for fluent configuration.</returns>
    public Rule<TFact1, TFact2> AddRule<TFact1, TFact2>(string name)
        where TFact1 : class
        where TFact2 : class
    {
        var rule = new Rule<TFact1, TFact2>("runtime", name);
        _tree.Add([typeof(TFact1), typeof(TFact2)], [rule]);
        return rule;
    }


    /// <summary>Adds a three-fact rule with the specified name.</summary>
    /// <typeparam name="TFact1">The first fact type.</typeparam>
    /// <typeparam name="TFact2">The second fact type.</typeparam>
    /// <typeparam name="TFact3">The third fact type.</typeparam>
    /// <param name="name">The name of the rule.</param>
    /// <returns>The newly created rule for fluent configuration.</returns>
    public Rule<TFact1, TFact2, TFact3> AddRule<TFact1, TFact2, TFact3>(string name)
        where TFact1 : class
        where TFact2 : class
        where TFact3 : class
    {
        var rule = new Rule<TFact1, TFact2, TFact3>("runtime", name);
        _tree.Add([typeof(TFact1), typeof(TFact2), typeof(TFact3)], [rule]);
        return rule;
    }



    /// <summary>Adds a four-fact rule with the specified name.</summary>
    /// <typeparam name="TFact1">The first fact type.</typeparam>
    /// <typeparam name="TFact2">The second fact type.</typeparam>
    /// <typeparam name="TFact3">The third fact type.</typeparam>
    /// <typeparam name="TFact4">The fourth fact type.</typeparam>
    /// <param name="name">The name of the rule.</param>
    /// <returns>The newly created rule for fluent configuration.</returns>
    public Rule<TFact1, TFact2, TFact3, TFact4> AddRule<TFact1, TFact2, TFact3, TFact4>(string name)
        where TFact1 : class
        where TFact2 : class
        where TFact3 : class
        where TFact4 : class
    {
        var rule = new Rule<TFact1, TFact2, TFact3, TFact4>("runtime", name);
        _tree.Add([typeof(TFact1), typeof(TFact2), typeof(TFact3), typeof(TFact4)], [rule]);
        return rule;
    }


    /// <summary>Removes all rules from this rule set.</summary>
    public void Clear()
    {
        _tree.Clear();
    }
}

