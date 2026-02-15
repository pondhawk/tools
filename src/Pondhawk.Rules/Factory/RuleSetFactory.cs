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
using Pondhawk.Rules.Listeners;
using Pondhawk.Rules.Tree;
namespace Pondhawk.Rules.Factory;

/// <summary>
/// Factory that discovers and loads rule builders from registered sources, producing configured <see cref="IRuleSet"/> instances.
/// </summary>
/// <remarks>
/// Uses <see cref="System.Lazy{T}"/> for thread-safe exactly-once initialization.
/// Register builder sources via <see cref="AddSources"/> or <see cref="AddAllSources"/>,
/// then call <see cref="Start"/> to discover and load all rule builders.
/// Use <see cref="GetRuleSet()"/> to obtain an <see cref="IRuleSet"/> for evaluation.
/// Integrates with <c>Pondhawk.Hosting</c> via <c>AddSingletonWithStart</c>.
/// </remarks>
/// <example>
/// <code>
/// var factory = new RuleSetFactory();
/// factory.AddSources(new RuleBuilderSource(typeof(OrderRules).Assembly));
/// factory.Start();
///
/// IRuleSet ruleSet = factory.GetRuleSet();
/// var result = ruleSet.Evaluate(order);
/// </code>
/// </example>
public sealed class RuleSetFactory
{


    private readonly Lazy<bool> _initialized;

    /// <summary>Initializes a new instance of the <see cref="RuleSetFactory"/> class.</summary>
    public RuleSetFactory()
    {
        _initialized = new Lazy<bool>(() => { DoStart(); return true; });
    }

    /// <summary>Gets or sets the factory used to create evaluation contexts.</summary>
    public IEvaluationContextFactory ContextFactory { get; set; }

    /// <summary>Gets or sets the factory used to create evaluation listeners.</summary>
    public IEvaluationListenerFactory ListenerFactory { get; set; }

    private Dictionary<string, IEnumerable<string>> CompositeNamespaces { get; } = new(StringComparer.Ordinal);

    /// <summary>Registers a composite namespace that groups multiple namespaces under a single name.</summary>
    /// <param name="name">The composite namespace name.</param>
    /// <param name="namespaces">The namespace strings to include in the composite.</param>
    public void RegisterCompositeNamespace(string name, IEnumerable<string> namespaces)
    {
        if (!_initialized.IsValueCreated)
            CompositeNamespaces[name] = namespaces;
    }



    private List<IRuleBuilderSource> Sources { get; } = [];


    /// <summary>Adds all rule builder sources from the collection.</summary>
    /// <param name="sources">The sources to add.</param>
    public void AddAllSources(IEnumerable<IRuleBuilderSource> sources)
    {
        Sources.AddRange(sources);
    }

    /// <summary>Adds one or more rule builder sources.</summary>
    /// <param name="sources">The sources to add.</param>
    public void AddSources(params IRuleBuilderSource[] sources)
    {
        Sources.AddRange(sources);
    }


    private RuleTree Tree { get; } = new();

    internal IRuleBase RuleBase => Tree;


    /// <summary>Discovers and loads all rule builders from registered sources. Thread-safe and runs at most once.</summary>
    public void Start() => _ = _initialized.Value;

    private void DoStart()
    {

        var builders = Sources.SelectMany(s => s.GetTypes()).Select(t => Activator.CreateInstance(t) as IBuilder);

        foreach (var b in builders.Where(b => b is not null))
            b.LoadRules(Tree);

    }

    /// <summary>Clears all loaded rules from the rule tree.</summary>
    public void Stop()
    {

        Tree.Clear();

    }



    /// <summary>Creates a new evaluation context, using the configured factories if available.</summary>
    /// <returns>A new evaluation context.</returns>
    public EvaluationContext BuildContext()
    {
        var context = ContextFactory?.CreateContext() ?? new EvaluationContext();

        if (ListenerFactory is not null)
            context.Listener = ListenerFactory.CreateListener();

        return context;
    }



    /// <summary>Gets a rule set that evaluates all loaded rules without namespace filtering.</summary>
    /// <returns>A new rule set.</returns>
    public IRuleSet GetRuleSet()
    {
        var ruleSet = new FactoryRuleSetImpl(Tree, [], ContextFactory);
        return ruleSet;
    }

    /// <summary>Gets a rule set filtered to the namespaces registered under the specified composite name.</summary>
    /// <param name="name">The composite namespace name previously registered via <see cref="RegisterCompositeNamespace"/>.</param>
    /// <returns>A namespace-filtered rule set.</returns>
    public IRuleSet GetRuleSetForComposite(string name)
    {
        if (!CompositeNamespaces.TryGetValue(name, out var composite))
            throw new InvalidOperationException($"Could not find Composite Namespace for given name ({name}).");

        var ruleSet = new FactoryRuleSetImpl(Tree, composite, ContextFactory);
        return ruleSet;
    }



    /// <summary>Gets a rule set filtered to the specified namespaces.</summary>
    /// <param name="namespaces">The namespace filters.</param>
    /// <returns>A namespace-filtered rule set.</returns>
    public IRuleSet GetRuleSet(params string[] namespaces)
    {
        var ruleSet = new FactoryRuleSetImpl(Tree, new HashSet<string>(namespaces, StringComparer.Ordinal), ContextFactory);
        return ruleSet;
    }



    /// <summary>Gets a rule set filtered to the specified namespaces.</summary>
    /// <param name="namespaces">The namespace filters.</param>
    /// <returns>A namespace-filtered rule set.</returns>
    public IRuleSet GetRuleSet(IEnumerable<string> namespaces)
    {
        var ruleSet = new FactoryRuleSetImpl(Tree, new HashSet<string>(namespaces, StringComparer.Ordinal), ContextFactory);
        return ruleSet;
    }


}
