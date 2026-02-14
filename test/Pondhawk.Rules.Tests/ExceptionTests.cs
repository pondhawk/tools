using Pondhawk.Rules.Exceptions;
using Pondhawk.Rules.Factory;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class ExceptionTests
{

    // ========== NoRulesEvaluatedException ==========

    [Fact]
    public void NoRulesEvaluatedException_ThrownWhenNoRulesMatch()
    {
        var ruleSet = new RuleSet();

        // No rules added, but ThrowNoRulesException is true by default
        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.AddFacts(new Person { Name = "Test" });

        Should.Throw<NoRulesEvaluatedException>(() => ruleSet.Evaluate(ctx));
    }

    [Fact]
    public void NoRulesEvaluatedException_ContainsResult()
    {
        var ruleSet = new RuleSet();

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.AddFacts(new Person { Name = "Test" });

        var ex = Should.Throw<NoRulesEvaluatedException>(() => ruleSet.Evaluate(ctx));

        ex.Result.ShouldNotBeNull();
        ex.Result.TotalEvaluated.ShouldBe(0);
    }

    [Fact]
    public void NoRulesEvaluatedException_Suppressed_WhenFlagIsFalse()
    {
        var ruleSet = new RuleSet();

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowNoRulesException = false;
        ctx.ThrowValidationException = false;
        ctx.AddFacts(new Person { Name = "Test" });

        var result = ruleSet.Evaluate(ctx);

        result.ShouldNotBeNull();
        result.TotalEvaluated.ShouldBe(0);
    }


    // ========== ViolationsExistException ==========

    [Fact]
    public void ViolationsExistException_ThrownWhenViolationsExist()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name is required");

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowNoRulesException = false;
        ctx.AddFacts(new Person { Name = "" });

        Should.Throw<ViolationsExistException>(() => ruleSet.Evaluate(ctx));
    }

    [Fact]
    public void ViolationsExistException_ContainsResult()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name is required");

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowNoRulesException = false;
        ctx.AddFacts(new Person { Name = "" });

        var ex = Should.Throw<ViolationsExistException>(() => ruleSet.Evaluate(ctx));

        ex.Result.ShouldNotBeNull();
        ex.Result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void ViolationsExistException_Violations_ReturnsOnlyViolationEvents()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name is required");

        // Also add a non-violation rule
        ruleSet.AddRule<Person>("info-rule")
            .Fire("Person evaluated: {0}", p => p.Name);

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowNoRulesException = false;
        ctx.AddFacts(new Person { Name = "" });

        var ex = Should.Throw<ViolationsExistException>(() => ruleSet.Evaluate(ctx));

        ex.Violations.ShouldNotBeNull();
        ex.Violations.All(v => v.Category == RuleEvent.EventCategory.Violation).ShouldBeTrue();
    }

    [Fact]
    public void ViolationsExistException_Suppressed_WhenFlagIsFalse()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("name-check")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name is required");

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.AddFacts(new Person { Name = "" });

        var result = ruleSet.Evaluate(ctx);

        result.ShouldNotBeNull();
        result.HasViolations.ShouldBeTrue();
    }


    // ========== EvaluationExhaustedException ==========

    [Fact]
    public void EvaluationExhaustedException_ThrownWhenMaxEvaluationsExceeded()
    {
        var ruleSet = new RuleSet();

        // Create a rule that always modifies to force re-evaluation
        ruleSet.AddRule<Person>("infinite")
            .If(p => p.Status != "done")
            .Then(p => { /* do nothing but signal modify */ })
            .Modifies(p => p);

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.MaxEvaluations = 10;
        ctx.MaxDuration = 60_000; // long timeout so count triggers first
        ctx.AddFacts(new Person { Name = "Test", Status = "new" });

        Should.Throw<EvaluationExhaustedException>(() => ruleSet.Evaluate(ctx));
    }

    [Fact]
    public void EvaluationExhaustedException_ContainsPartialResults()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("infinite")
            .If(p => p.Status != "done")
            .Then(p => { })
            .Modifies(p => p);

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.MaxEvaluations = 10;
        ctx.MaxDuration = 60_000;
        ctx.AddFacts(new Person { Name = "Test", Status = "new" });

        var ex = Should.Throw<EvaluationExhaustedException>(() => ruleSet.Evaluate(ctx));

        ex.Result.ShouldNotBeNull();
        ex.Result.TotalEvaluated.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void EvaluationExhaustedException_ThrownWhenMaxDurationExceeded()
    {
        var ruleSet = new RuleSet();

        // Create a rule that sleeps and modifies to force re-evaluation
        ruleSet.AddRule<Person>("slow-infinite")
            .If(p => p.Status != "done")
            .Then(p => Thread.Sleep(50))
            .Modifies(p => p);

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.MaxEvaluations = 500000;
        ctx.MaxDuration = 100; // very short timeout
        ctx.AddFacts(new Person { Name = "Test", Status = "new" });

        Should.Throw<EvaluationExhaustedException>(() => ruleSet.Evaluate(ctx));
    }


    // ========== MaxViolations ==========

    [Fact]
    public void MaxViolations_StopsEvaluationWhenLimitReached()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("v1")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        ruleSet.AddValidation<Person>("v2")
            .Assert<string>(p => p.Email)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Email required");

        ruleSet.AddValidation<Person>("v3")
            .Assert<string>(p => p.Status)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Status required");

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.MaxViolations = 1;
        ctx.AddFacts(new Person { Name = "", Email = "", Status = "" });

        var result = ruleSet.Evaluate(ctx);

        result.ViolationCount.ShouldBeLessThanOrEqualTo(2);
    }

}
