using Pondhawk.Exceptions;
using Pondhawk.Rules.Factory;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class RuleAdvancedTests
{

    private static EvaluationResults EvaluateSafe(RuleSet ruleSet, params object[] facts)
    {
        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.AddAllFacts(facts);
        return ruleSet.Evaluate(ctx);
    }


    // ========== Cascade ==========

    [Fact]
    public void Cascade_InsertsChildFact()
    {
        var ruleSet = new RuleSet();
        var addressProcessed = false;

        ruleSet.AddRule<Person>("cascade")
            .If(p => p.Addresses.Count > 0)
            .Cascade(p => p.Addresses[0]);

        ruleSet.AddRule<Address>("addr-rule")
            .Fire(a => { addressProcessed = true; });

        var person = new Person
        {
            Name = "Alice",
            Addresses = [new Address { Street = "123 Main", City = "Testville" }]
        };

        EvaluateSafe(ruleSet, person);

        addressProcessed.ShouldBeTrue();
    }

    [Fact]
    public void CascadeAll_InsertsAllChildren()
    {
        var ruleSet = new RuleSet();
        var cities = new List<string>();

        ruleSet.AddRule<Person>("cascade-all")
            .If(p => p.Addresses.Count > 0)
            .CascadeAll(p => p.Addresses);

        ruleSet.AddRule<Address>("addr-rule")
            .Fire(a => cities.Add(a.City));

        var person = new Person
        {
            Name = "Alice",
            Addresses =
            [
                new Address { Street = "1st", City = "Alpha" },
                new Address { Street = "2nd", City = "Beta" }
            ]
        };

        EvaluateSafe(ruleSet, person);

        cities.Count.ShouldBe(2);
        cities.ShouldContain("Alpha");
        cities.ShouldContain("Beta");
    }


    // ========== OtherwiseAffirm / OtherwiseVeto ==========

    [Fact]
    public void OtherwiseAffirm_AddsAffirmationWhenConditionFails()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("oa")
            .If(p => p.Age >= 100)
            .OtherwiseAffirm(15);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", Age = 25 });

        result.TotalAffirmations.ShouldBe(15);
    }

    [Fact]
    public void OtherwiseVeto_AddsVetoWhenConditionFails()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("ov")
            .If(p => p.Age >= 100)
            .OtherwiseVeto(8);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", Age = 25 });

        result.TotalVetos.ShouldBe(8);
    }

    [Fact]
    public void OtherwiseAffirm_DoesNotFireWhenConditionPasses()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("oa")
            .If(p => p.Age >= 18)
            .OtherwiseAffirm(15);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", Age = 25 });

        result.TotalAffirmations.ShouldBe(0);
    }


    // ========== Otherwise with message templates ==========

    [Fact]
    public void Otherwise_MessageTemplate_CreatesEvent()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("om")
            .If(p => p.Age >= 100)
            .Otherwise("{0} is not centenarian", p => p.Name);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Alice", Age = 25 });

        result.Events.Count.ShouldBe(1);
        result.Events.First().Explanation.ShouldBe("Alice is not centenarian");
    }

    [Fact]
    public void Otherwise_GroupedMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("ogm")
            .If(p => p.Age >= 100)
            .Otherwise("age-group", "{0} not old enough", p => p.Name);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Alice", Age = 25 });

        result.Events.First().Group.ShouldBe("age-group");
    }

    [Fact]
    public void Otherwise_CategoryMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("ocm")
            .If(p => p.Age >= 100)
            .Otherwise(EventDetail.EventCategory.Violation, "grp", "Too young: {0}", p => p.Name);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Alice", Age = 25 });

        var evt = result.Events.First();
        evt.Category.ShouldBe(EventDetail.EventCategory.Violation);
    }


    // ========== Fire with categories ==========

    [Fact]
    public void Fire_ViolationCategory()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("violation")
            .Fire(EventDetail.EventCategory.Violation, "security", "Unauthorized: {0}", p => p.Name);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Alice" });

        var evt = result.Events.First();
        evt.Category.ShouldBe(EventDetail.EventCategory.Violation);
        evt.Group.ShouldBe("security");
    }

    [Fact]
    public void Fire_WarningCategory()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("warning")
            .Fire(EventDetail.EventCategory.Warning, "trace", "Processing {0}", p => p.Name);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Alice" });

        result.Events.First().Category.ShouldBe(EventDetail.EventCategory.Warning);
    }


    // ========== Then with category ==========

    [Fact]
    public void Then_CategoryMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("cat")
            .If(p => p.Age >= 18)
            .Then(EventDetail.EventCategory.Warning, "age", "Adult: {0}", p => p.Name);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Alice", Age = 25 });

        var evt = result.Events.First();
        evt.Category.ShouldBe(EventDetail.EventCategory.Warning);
    }


    // ========== FireOnce with modify ==========

    [Fact]
    public void FireOnce_WithModify_StopsReEvaluation()
    {
        var ruleSet = new RuleSet();
        var fireCount = 0;

        ruleSet.AddRule<Person>("once-mod")
            .FireOnce()
            .If(p => p.Status == "new")
            .Then(p => { p.Status = "done"; fireCount++; })
            .Modifies(p => p);

        EvaluateSafe(ruleSet, new Person { Name = "Test", Status = "new" });

        // Should fire once even though Modifies triggers re-eval — FireOnce prevents it
        fireCount.ShouldBe(1);
    }


    // ========== FireAlways ==========

    [Fact]
    public void FireAlways_OverridesFireOnce()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("always")
            .FireOnce()
            .FireAlways()
            .Fire(p => { });

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test" });

        result.TotalFired.ShouldBeGreaterThanOrEqualTo(1);
    }


    // ========== Inception + Expiration window ==========

    [Fact]
    public void InceptionAndExpiration_WithinWindow_Fires()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person>("window")
            .WithInception(DateTime.Now.AddDays(-1))
            .WithExpiration(DateTime.Now.AddDays(1))
            .Fire(p => { fired = true; });

        EvaluateSafe(ruleSet, new Person { Name = "Test" });

        fired.ShouldBeTrue();
    }


    // ========== Null parameter handling in message ==========

    [Fact]
    public void Then_NullParameter_RendersAsNull()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("null-msg")
            .Fire("Value is {0}", p => (object)null);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test" });

        result.Events.First().Explanation.ShouldBe("Value is null");
    }

}
