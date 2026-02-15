using Pondhawk.Rules.Factory;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class MultiFactRuleTests
{

    private static EvaluationResults EvaluateSafe(RuleSet ruleSet, params object[] facts)
    {
        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.AddAllFacts(facts);
        return ruleSet.Evaluate(ctx);
    }


    // ========== Two-fact rules ==========

    [Fact]
    public void TwoFactRule_BothMatch_FiresConsequence()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order>("match")
            .When((p, o) => p.Name == o.CustomerName)
            .Then((p, o) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        fired.ShouldBeTrue();
    }

    [Fact]
    public void TwoFactRule_OneFailsCondition_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order>("no-match")
            .When((p, o) => p.Name == o.CustomerName)
            .Then((p, o) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Bob" });

        fired.ShouldBeFalse();
    }

    [Fact]
    public void TwoFactRule_MessageTemplate_InterpolatesBothFacts()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("msg")
            .Fire("{0} ordered {1}", (p, o) => p.Name, (p, o) => o.OrderId);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 42, CustomerName = "Alice" });

        result.Events.Count.ShouldBe(1);
        result.Events.First().Message.ShouldBe("Alice ordered 42");
    }

    [Fact]
    public void TwoFactRule_Otherwise_FiresWhenConditionFails()
    {
        var ruleSet = new RuleSet();
        var firedOtherwise = false;

        ruleSet.AddRule<Person, Order>("otherwise")
            .When((p, o) => p.Name == o.CustomerName)
            .Otherwise((p, o) => { firedOtherwise = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Bob" });

        firedOtherwise.ShouldBeTrue();
    }

    [Fact]
    public void TwoFactRule_Otherwise_DoesNotFireWhenConditionPasses()
    {
        var ruleSet = new RuleSet();
        var firedOtherwise = false;

        ruleSet.AddRule<Person, Order>("otherwise")
            .When((p, o) => p.Name == o.CustomerName)
            .Otherwise((p, o) => { firedOtherwise = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        firedOtherwise.ShouldBeFalse();
    }

    [Fact]
    public void TwoFactRule_AffirmVeto_Scoring()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("affirm")
            .When((p, o) => p.Name == o.CustomerName)
            .ThenAffirm(10);

        ruleSet.AddRule<Person, Order>("veto")
            .When((p, o) => o.Total <= 0)
            .ThenVeto(5);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice", Total = 0 });

        result.TotalAffirmations.ShouldBe(10);
        result.TotalVetos.ShouldBe(5);
        result.Score.ShouldBe(5);
    }

    [Fact]
    public void TwoFactRule_OtherwiseAffirm_AddsAffirmationWhenConditionFails()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("oa")
            .When((p, o) => p.Name == o.CustomerName)
            .OtherwiseAffirm(7);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Bob" });

        result.TotalAffirmations.ShouldBe(7);
    }

    [Fact]
    public void TwoFactRule_OtherwiseVeto_AddsVetoWhenConditionFails()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("ov")
            .When((p, o) => p.Name == o.CustomerName)
            .OtherwiseVeto(3);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Bob" });

        result.TotalVetos.ShouldBe(3);
    }

    [Fact]
    public void TwoFactRule_FireAffirm_Unconditional()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("fa")
            .FireAffirm(15);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        result.TotalAffirmations.ShouldBe(15);
    }

    [Fact]
    public void TwoFactRule_FireVeto_Unconditional()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("fv")
            .FireVeto(8);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        result.TotalVetos.ShouldBe(8);
    }

    [Fact]
    public void TwoFactRule_Modifies_TriggersForwardChaining()
    {
        var ruleSet = new RuleSet();
        var secondRuleFired = false;

        ruleSet.AddRule<Person, Order>("modifier")
            .WithSalience(100)
            .When((p, o) => o.Status == "new")
            .Then((p, o) => { o.Status = "processed"; })
            .Modifies((p, o) => o);

        ruleSet.AddRule<Person, Order>("after")
            .WithSalience(200)
            .When((p, o) => o.Status == "processed")
            .Then((p, o) => { secondRuleFired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice", Status = "new" });

        secondRuleFired.ShouldBeTrue();
    }

    [Fact]
    public void TwoFactRule_WithSalienceAndMutex()
    {
        var ruleSet = new RuleSet();
        var firedRules = new List<string>();

        ruleSet.AddRule<Person, Order>("a")
            .WithSalience(100)
            .InMutex("group")
            .Fire((p, o) => firedRules.Add("A"));

        ruleSet.AddRule<Person, Order>("b")
            .WithSalience(200)
            .InMutex("group")
            .Fire((p, o) => firedRules.Add("B"));

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        firedRules.Count.ShouldBe(1);
        firedRules[0].ShouldBe("A");
    }

    [Fact]
    public void TwoFactRule_FireOnce_PreventsReFiring()
    {
        var ruleSet = new RuleSet();
        var fireCount = 0;

        ruleSet.AddRule<Person, Order>("once")
            .FireOnce()
            .Fire((p, o) => fireCount++);

        // Two orders — each creates a different tuple with the same person, but each is unique
        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Order { OrderId = 2, CustomerName = "Alice" });

        fireCount.ShouldBe(2); // Each unique (Person, Order) tuple fires once
    }

    [Fact]
    public void TwoFactRule_FireAlways_AllowsReFiring()
    {
        var ruleSet = new RuleSet();
        var fireCount = 0;

        ruleSet.AddRule<Person, Order>("always")
            .FireAlways()
            .Fire((p, o) => fireCount++);

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        fireCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void TwoFactRule_InceptionExpiration_WithinWindow_Fires()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order>("windowed")
            .WithInception(DateTime.Now.AddDays(-1))
            .WithExpiration(DateTime.Now.AddDays(1))
            .Fire((p, o) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        fired.ShouldBeTrue();
    }

    [Fact]
    public void TwoFactRule_FutureInception_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order>("future")
            .WithInception(DateTime.Now.AddDays(1))
            .Fire((p, o) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        fired.ShouldBeFalse();
    }

    [Fact]
    public void TwoFactRule_PastExpiration_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order>("expired")
            .WithExpiration(DateTime.Now.AddDays(-1))
            .Fire((p, o) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        fired.ShouldBeFalse();
    }

    [Fact]
    public void TwoFactRule_GroupedMessage_SetsGroup()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("grp")
            .Fire("orders", "{0} placed order", (p, o) => p.Name);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        result.Events.First().Group.ShouldBe("orders");
    }

    [Fact]
    public void TwoFactRule_CategoryMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("cat")
            .Fire(RuleEvent.EventCategory.Violation, "grp", "Bad order from {0}", (p, o) => p.Name);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        var evt = result.Events.First();
        evt.Category.ShouldBe(RuleEvent.EventCategory.Violation);
        evt.Message.ShouldBe("Bad order from Alice");
    }

    [Fact]
    public void TwoFactRule_OtherwiseMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("om")
            .When((p, o) => false)
            .Otherwise("{0} did not match", (p, o) => p.Name);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        result.Events.First().Message.ShouldBe("Alice did not match");
    }

    [Fact]
    public void TwoFactRule_OtherwiseGroupMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("ogm")
            .When((p, o) => false)
            .Otherwise("mismatch", "{0} did not match", (p, o) => p.Name);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        var evt = result.Events.First();
        evt.Group.ShouldBe("mismatch");
    }

    [Fact]
    public void TwoFactRule_OtherwiseCategoryMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("ocm")
            .When((p, o) => false)
            .Otherwise(RuleEvent.EventCategory.Violation, "grp", "No match for {0}", (p, o) => p.Name);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        var evt = result.Events.First();
        evt.Category.ShouldBe(RuleEvent.EventCategory.Violation);
    }

    [Fact]
    public void TwoFactRule_And_AddsMultipleConditions()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order>("multi-cond")
            .When((p, o) => p.Name == o.CustomerName)
            .And((p, o) => o.Total > 100)
            .Then((p, o) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice", Total = 50 });

        fired.ShouldBeFalse();
    }

    [Fact]
    public void TwoFactRule_ThenGroupedMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("tgm")
            .When((p, o) => true)
            .Then("mygroup", "{0} ok", (p, o) => p.Name);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        var evt = result.Events.First();
        evt.Group.ShouldBe("mygroup");
    }

    [Fact]
    public void TwoFactRule_ThenCategoryMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order>("tcm")
            .When((p, o) => true)
            .Then(RuleEvent.EventCategory.Warning, "warn", "{0}", (p, o) => p.Name);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" });

        var evt = result.Events.First();
        evt.Category.ShouldBe(RuleEvent.EventCategory.Warning);
    }


    // ========== Three-fact rules ==========

    [Fact]
    public void ThreeFactRule_AllMatch_Fires()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order, Account>("three")
            .When((p, o, a) => p.Name == o.CustomerName && a.Balance > 0)
            .Then((p, o, a) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 });

        fired.ShouldBeTrue();
    }

    [Fact]
    public void ThreeFactRule_PartialMatch_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order, Account>("three")
            .When((p, o, a) => p.Name == o.CustomerName && a.Balance > 0)
            .Then((p, o, a) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Bob" },
            new Account { AccountId = "A1", Balance = 100 });

        fired.ShouldBeFalse();
    }

    [Fact]
    public void ThreeFactRule_AffirmVeto()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order, Account>("ta")
            .FireAffirm(20);

        ruleSet.AddRule<Person, Order, Account>("tv")
            .FireVeto(5);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 });

        result.Score.ShouldBe(15);
    }

    [Fact]
    public void ThreeFactRule_Otherwise_Fires()
    {
        var ruleSet = new RuleSet();
        var firedOtherwise = false;

        ruleSet.AddRule<Person, Order, Account>("oth")
            .When((p, o, a) => false)
            .Otherwise((p, o, a) => { firedOtherwise = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 });

        firedOtherwise.ShouldBeTrue();
    }

    [Fact]
    public void ThreeFactRule_MessageTemplate()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order, Account>("msg")
            .Fire("{0} order {1} account {2}",
                (p, o, a) => p.Name,
                (p, o, a) => o.OrderId,
                (p, o, a) => a.AccountId);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 42, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 });

        result.Events.First().Message.ShouldBe("Alice order 42 account A1");
    }

    [Fact]
    public void ThreeFactRule_Modifies_TriggersForwardChaining()
    {
        var ruleSet = new RuleSet();
        var secondFired = false;

        ruleSet.AddRule<Person, Order, Account>("mod")
            .WithSalience(100)
            .When((p, o, a) => a.Type == "new")
            .Then((p, o, a) => { a.Type = "processed"; })
            .Modifies((p, o, a) => a);

        ruleSet.AddRule<Person, Order, Account>("after")
            .WithSalience(200)
            .When((p, o, a) => a.Type == "processed")
            .Then((p, o, a) => { secondFired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100, Type = "new" });

        secondFired.ShouldBeTrue();
    }

    [Fact]
    public void ThreeFactRule_SalienceAndMutex()
    {
        var ruleSet = new RuleSet();
        var firedRules = new List<string>();

        ruleSet.AddRule<Person, Order, Account>("a")
            .WithSalience(100)
            .InMutex("grp")
            .Fire((p, o, a) => firedRules.Add("A"));

        ruleSet.AddRule<Person, Order, Account>("b")
            .WithSalience(200)
            .InMutex("grp")
            .Fire((p, o, a) => firedRules.Add("B"));

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 });

        firedRules.Count.ShouldBe(1);
        firedRules[0].ShouldBe("A");
    }

    [Fact]
    public void ThreeFactRule_OtherwiseAffirm()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order, Account>("oa")
            .When((p, o, a) => false)
            .OtherwiseAffirm(12);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 });

        result.TotalAffirmations.ShouldBe(12);
    }

    [Fact]
    public void ThreeFactRule_OtherwiseVeto()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order, Account>("ov")
            .When((p, o, a) => false)
            .OtherwiseVeto(9);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 });

        result.TotalVetos.ShouldBe(9);
    }


    // ========== Four-fact rules ==========

    [Fact]
    public void FourFactRule_AllMatch_Fires()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order, Account, Policy>("four")
            .When((p, o, a, pol) => p.Name == o.CustomerName && a.Balance > 0 && pol.Priority > 0)
            .Then((p, o, a, pol) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 },
            new Policy { PolicyId = "P1", Priority = 1 });

        fired.ShouldBeTrue();
    }

    [Fact]
    public void FourFactRule_PartialMatch_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person, Order, Account, Policy>("four")
            .When((p, o, a, pol) => pol.Priority > 10)
            .Then((p, o, a, pol) => { fired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 },
            new Policy { PolicyId = "P1", Priority = 1 });

        fired.ShouldBeFalse();
    }

    [Fact]
    public void FourFactRule_AffirmVeto()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order, Account, Policy>("aff")
            .FireAffirm(30);

        ruleSet.AddRule<Person, Order, Account, Policy>("vet")
            .FireVeto(10);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 },
            new Policy { PolicyId = "P1", Priority = 1 });

        result.Score.ShouldBe(20);
    }

    [Fact]
    public void FourFactRule_MessageTemplate()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order, Account, Policy>("msg")
            .Fire("{0} {1} {2} {3}",
                (p, o, a, pol) => p.Name,
                (p, o, a, pol) => o.OrderId,
                (p, o, a, pol) => a.AccountId,
                (p, o, a, pol) => pol.PolicyId);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 42, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 },
            new Policy { PolicyId = "P1", Priority = 1 });

        result.Events.First().Message.ShouldBe("Alice 42 A1 P1");
    }

    [Fact]
    public void FourFactRule_Otherwise_Fires()
    {
        var ruleSet = new RuleSet();
        var firedOtherwise = false;

        ruleSet.AddRule<Person, Order, Account, Policy>("oth")
            .When((p, o, a, pol) => false)
            .Otherwise((p, o, a, pol) => { firedOtherwise = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 },
            new Policy { PolicyId = "P1", Priority = 1 });

        firedOtherwise.ShouldBeTrue();
    }

    [Fact]
    public void FourFactRule_Modifies_TriggersForwardChaining()
    {
        var ruleSet = new RuleSet();
        var secondFired = false;

        ruleSet.AddRule<Person, Order, Account, Policy>("mod")
            .WithSalience(100)
            .When((p, o, a, pol) => pol.Category == "new")
            .Then((p, o, a, pol) => { pol.Category = "done"; })
            .Modifies((p, o, a, pol) => pol);

        ruleSet.AddRule<Person, Order, Account, Policy>("after")
            .WithSalience(200)
            .When((p, o, a, pol) => pol.Category == "done")
            .Then((p, o, a, pol) => { secondFired = true; });

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 },
            new Policy { PolicyId = "P1", Priority = 1, Category = "new" });

        secondFired.ShouldBeTrue();
    }

    [Fact]
    public void FourFactRule_SalienceOrdering()
    {
        var ruleSet = new RuleSet();
        var order = new List<string>();

        ruleSet.AddRule<Person, Order, Account, Policy>("second")
            .WithSalience(200)
            .Fire((p, o, a, pol) => order.Add("second"));

        ruleSet.AddRule<Person, Order, Account, Policy>("first")
            .WithSalience(100)
            .Fire((p, o, a, pol) => order.Add("first"));

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 },
            new Policy { PolicyId = "P1", Priority = 1 });

        order[0].ShouldBe("first");
        order[1].ShouldBe("second");
    }

    [Fact]
    public void FourFactRule_OtherwiseAffirm()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order, Account, Policy>("oa")
            .When((p, o, a, pol) => false)
            .OtherwiseAffirm(25);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 },
            new Policy { PolicyId = "P1", Priority = 1 });

        result.TotalAffirmations.ShouldBe(25);
    }

    [Fact]
    public void FourFactRule_OtherwiseVeto()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person, Order, Account, Policy>("ov")
            .When((p, o, a, pol) => false)
            .OtherwiseVeto(11);

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Account { AccountId = "A1", Balance = 100 },
            new Policy { PolicyId = "P1", Priority = 1 });

        result.TotalVetos.ShouldBe(11);
    }

}
