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
    public async Task RuleSetFactory_Start_LoadsBuilders()
    {
        var factory = new RuleSetFactory();

        var source = new RuleBuilderSource();
        source.AddTypes(typeof(PersonAgeRules));

        factory.AddSources(source);

        await factory.StartAsync();

        var ruleSet = factory.GetRuleSet();
        var adult = new Person { Name = "Alice", Age = 25 };
        var result = ruleSet.Evaluate(adult);

        result.TotalFired.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task RuleSetFactory_Stop_ClearsRules()
    {
        var factory = new RuleSetFactory();

        var source = new RuleBuilderSource();
        source.AddTypes(typeof(PersonAgeRules));

        factory.AddSources(source);

        await factory.StartAsync();

        factory.Stop();

        var ruleSet = factory.GetRuleSet();
        var result = ruleSet.Evaluate(new Person { Name = "Test", Age = 25 });

        result.TotalFired.ShouldBe(0);
    }

    [Fact]
    public async Task RuleSetFactory_GetRuleSetWithNamespaces_FiltersRules()
    {
        var factory = new RuleSetFactory();

        var source = new RuleBuilderSource();
        source.AddTypes(typeof(PersonAgeRules));

        factory.AddSources(source);

        await factory.StartAsync();

        // Get ruleset with a non-matching namespace
        var ruleSet = factory.GetRuleSet("some.other.namespace");
        var result = ruleSet.Evaluate(new Person { Name = "Test", Age = 25 });

        result.TotalFired.ShouldBe(0);
    }

    [Fact]
    public async Task RuleSetFactory_StartAsync_Idempotent()
    {
        var factory = new RuleSetFactory();

        var source = new RuleBuilderSource();
        source.AddTypes(typeof(PersonAgeRules));

        factory.AddSources(source);

        await factory.StartAsync();
        await factory.StartAsync(); // second call should be no-op

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
