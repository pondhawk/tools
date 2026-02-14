using Pondhawk.Rules.Factory;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class RuleSetExtensionsTests
{

    // ========== Evaluate(params) extension ==========

    [Fact]
    public void Evaluate_ParamsOverload_ReturnsResults()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("test")
            .Fire(p => { });

        // Cast to IRuleSet to invoke the extension method (not the instance method)
        IRuleSet rs = ruleSet;
        var result = rs.Evaluate(new Person { Name = "Alice" });

        result.ShouldNotBeNull();
        result.TotalFired.ShouldBe(1);
    }

    [Fact]
    public void Evaluate_ParamsOverload_DoesNotThrowOnViolations()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        IRuleSet rs = ruleSet;
        var result = rs.Evaluate(new Person { Name = "" });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void Evaluate_ParamsOverload_DoesNotThrowOnNoRules()
    {
        var ruleSet = new RuleSet();

        IRuleSet rs = ruleSet;
        var result = rs.Evaluate(new Person { Name = "Test" });

        result.ShouldNotBeNull();
        result.TotalEvaluated.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_ParamsOverload_MultipleFacts()
    {
        var ruleSet = new RuleSet();
        var firedNames = new List<string>();

        ruleSet.AddRule<Person>("collect")
            .Fire(p => firedNames.Add(p.Name));

        IRuleSet rs = ruleSet;
        rs.Evaluate(new Person { Name = "Alice" }, new Person { Name = "Bob" });

        firedNames.Count.ShouldBe(2);
    }


    // ========== Evaluate(IEnumerable) extension ==========

    [Fact]
    public void Evaluate_IEnumerableOverload_ReturnsResults()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("test")
            .Fire(p => { });

        IRuleSet rs = ruleSet;
        IEnumerable<object> facts = [new Person { Name = "Alice" }];
        var result = rs.Evaluate(facts);

        result.ShouldNotBeNull();
        result.TotalFired.ShouldBe(1);
    }

    [Fact]
    public void Evaluate_IEnumerableOverload_DoesNotThrowOnViolations()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        IRuleSet rs = ruleSet;
        IEnumerable<object> facts = [new Person { Name = "" }];
        var result = rs.Evaluate(facts);

        result.HasViolations.ShouldBeTrue();
    }


    // ========== TryValidate(single) ==========

    [Fact]
    public void TryValidate_ValidFact_ReturnsTrue_EmptyViolations()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        var valid = ruleSet.TryValidate(new Person { Name = "Alice" }, out var violations);

        valid.ShouldBeTrue();
        violations.ShouldBeEmpty();
    }

    [Fact]
    public void TryValidate_InvalidFact_ReturnsFalse_PopulatedViolations()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        var valid = ruleSet.TryValidate(new Person { Name = "" }, out var violations);

        valid.ShouldBeFalse();
        violations.ShouldNotBeEmpty();
        violations[0].Message.ShouldBe("Name required");
    }


    // ========== TryValidate(IEnumerable) ==========

    [Fact]
    public void TryValidate_IEnumerable_AllValid_ReturnsTrue()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        IEnumerable<object> subjects = [new Person { Name = "Alice" }, new Person { Name = "Bob" }];
        var valid = ruleSet.TryValidate(subjects, out var violations);

        valid.ShouldBeTrue();
        violations.ShouldBeEmpty();
    }

    [Fact]
    public void TryValidate_IEnumerable_SomeInvalid_ReturnsFalse()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        IEnumerable<object> subjects = [new Person { Name = "Alice" }, new Person { Name = "" }];
        var valid = ruleSet.TryValidate(subjects, out var violations);

        valid.ShouldBeFalse();
        violations.ShouldNotBeEmpty();
    }

}
