using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Factory;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;


// A concrete RuleBuilder for testing the assembly-discovery pattern
public class PersonAgeRules : RuleBuilder<Person>
{
    public PersonAgeRules()
    {
        Rule()
            .If(p => p.Age >= 18)
            .Then(p => { });

        Rule("minor")
            .If(p => p.Age < 18)
            .Then("minors", "Person {0} is a minor", p => p.Name);
    }
}


public class RuleSetFactoryTests
{

    [Fact]
    public void RuleSetFactory_Start_LoadsBuilders()
    {
        var factory = new RuleSetFactory();

        var source = new RuleBuilderSource();
        source.AddTypes(typeof(PersonAgeRules));

        factory.AddSources(source);

        factory.Start();

        var ruleSet = factory.GetRuleSet();
        var adult = new Person { Name = "Alice", Age = 25 };
        var result = ruleSet.Evaluate(adult);

        result.TotalFired.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void RuleSetFactory_Stop_ClearsRules()
    {
        var factory = new RuleSetFactory();

        var source = new RuleBuilderSource();
        source.AddTypes(typeof(PersonAgeRules));

        factory.AddSources(source);

        factory.Start();

        factory.Stop();

        var ruleSet = factory.GetRuleSet();
        var result = ruleSet.Evaluate(new Person { Name = "Test", Age = 25 });

        result.TotalFired.ShouldBe(0);
    }

    [Fact]
    public void RuleSetFactory_GetRuleSetWithNamespaces_FiltersRules()
    {
        var factory = new RuleSetFactory();

        var source = new RuleBuilderSource();
        source.AddTypes(typeof(PersonAgeRules));

        factory.AddSources(source);

        factory.Start();

        // Get ruleset with a non-matching namespace
        var ruleSet = factory.GetRuleSet("some.other.namespace");
        var result = ruleSet.Evaluate(new Person { Name = "Test", Age = 25 });

        result.TotalFired.ShouldBe(0);
    }

    [Fact]
    public void RuleSetFactory_Start_Idempotent()
    {
        var factory = new RuleSetFactory();

        var source = new RuleBuilderSource();
        source.AddTypes(typeof(PersonAgeRules));

        factory.AddSources(source);

        factory.Start();
        factory.Start(); // second call should be no-op

        var ruleSet = factory.GetRuleSet();
        var result = ruleSet.Evaluate(new Person { Name = "Test", Age = 25 });

        result.TotalFired.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void BuildContext_ReturnsNewContext()
    {
        var factory = new RuleSetFactory();

        var ctx = factory.BuildContext();

        ctx.ShouldNotBeNull();
        ctx.ShouldBeOfType<EvaluationContext>();
    }

}
