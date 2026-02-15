using Pondhawk.Rules.Factory;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class DecideTests
{

    // ========== Decide with default threshold ==========

    [Fact]
    public void Decide_ScoreAboveDefaultThreshold_ReturnsTrue()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var result = ruleSet.Decide(new Person { Name = "Test" });

        result.ShouldBeTrue();
    }

    [Fact]
    public void Decide_ScoreAtDefaultThreshold_ReturnsTrue()
    {
        var ruleSet = new RuleSet();

        // Default threshold is 0, so score of 0 meets threshold
        ruleSet.AddRule<Person>("neutral").Fire(p => { });

        var result = ruleSet.Decide(new Person { Name = "Test" });

        result.ShouldBeTrue();
    }

    [Fact]
    public void Decide_NegativeScore_ReturnsFalse()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("veto").FireVeto(5);

        var result = ruleSet.Decide(new Person { Name = "Test" });

        result.ShouldBeFalse();
    }


    // ========== Decide with custom DecisionThreshold ==========

    [Fact]
    public void Decide_CustomThreshold_ScoreAbove_ReturnsTrue()
    {
        var ruleSet = new RuleSet();
        ruleSet.DecisionThreshold = 5;

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var result = ruleSet.Decide(new Person { Name = "Test" });

        result.ShouldBeTrue();
    }

    [Fact]
    public void Decide_CustomThreshold_ScoreBelow_ReturnsFalse()
    {
        var ruleSet = new RuleSet();
        ruleSet.DecisionThreshold = 15;

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var result = ruleSet.Decide(new Person { Name = "Test" });

        result.ShouldBeFalse();
    }

    [Fact]
    public void Decide_CustomThreshold_ScoreEqual_ReturnsTrue()
    {
        var ruleSet = new RuleSet();
        ruleSet.DecisionThreshold = 10;

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var result = ruleSet.Decide(new Person { Name = "Test" });

        result.ShouldBeTrue();
    }


    // ========== Decide with explicit threshold overload ==========

    [Fact]
    public void Decide_ExplicitThreshold_ScoreAbove_ReturnsTrue()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var result = ruleSet.Decide(5, new Person { Name = "Test" });

        result.ShouldBeTrue();
    }

    [Fact]
    public void Decide_ExplicitThreshold_ScoreBelow_ReturnsFalse()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var result = ruleSet.Decide(15, new Person { Name = "Test" });

        result.ShouldBeFalse();
    }

    [Fact]
    public void Decide_ExplicitThreshold_OverridesDecisionThreshold()
    {
        var ruleSet = new RuleSet();
        ruleSet.DecisionThreshold = 100; // High property threshold

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        // Using the overload with explicit threshold=5 should override the property
        var result = ruleSet.Decide(5, new Person { Name = "Test" });

        result.ShouldBeTrue();
    }


    // ========== Predicate property ==========

    [Fact]
    public void Predicate_WrapsDecide()
    {
        var ruleSet = new RuleSet();
        ruleSet.DecisionThreshold = 5;

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var predicate = ruleSet.Predicate;
        predicate.ShouldNotBeNull();

        var result = predicate(new Person { Name = "Test" });
        result.ShouldBeTrue();
    }

    [Fact]
    public void Predicate_ReturnsFalse_WhenScoreBelowThreshold()
    {
        var ruleSet = new RuleSet();
        ruleSet.DecisionThreshold = 20;

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var result = ruleSet.Predicate(new Person { Name = "Test" });
        result.ShouldBeFalse();
    }


    // ========== DecisionThreshold property ==========

    [Fact]
    public void DecisionThreshold_DefaultIsZero()
    {
        var ruleSet = new RuleSet();

        ruleSet.DecisionThreshold.ShouldBe(0);
    }

    [Fact]
    public void DecisionThreshold_CanBeSet()
    {
        var ruleSet = new RuleSet();

        ruleSet.DecisionThreshold = 42;

        ruleSet.DecisionThreshold.ShouldBe(42);
    }

}
