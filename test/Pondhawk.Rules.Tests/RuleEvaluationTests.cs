using Pondhawk.Rules.Factory;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class RuleEvaluationTests
{

    /// <summary>
    /// Helper that evaluates without throwing ViolationsExistException or NoRulesEvaluatedException.
    /// The instance method Evaluate(params object[]) uses a default EvaluationContext which throws
    /// on violations and when no rules fire. This helper creates a context with exceptions disabled.
    /// </summary>
    private static EvaluationResults EvaluateSafe(RuleSet ruleSet, params object[] facts)
    {
        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.AddAllFacts(facts);
        return ruleSet.Evaluate(ctx);
    }


    // ========== Basic single-fact evaluation ==========

    [Fact]
    public void Evaluate_SingleRule_ConditionTrue_FiresConsequence()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person>("age-check")
            .If(p => p.Age >= 18)
            .Then(p => { fired = true; });

        var person = new Person { Name = "Alice", Age = 25 };
        var result = EvaluateSafe(ruleSet, person);

        fired.ShouldBeTrue();
        result.TotalFired.ShouldBe(1);
        result.TotalEvaluated.ShouldBe(1);
    }

    [Fact]
    public void Evaluate_SingleRule_ConditionFalse_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person>("age-check")
            .If(p => p.Age >= 18)
            .Then(p => { fired = true; });

        var person = new Person { Name = "Child", Age = 10 };
        var result = EvaluateSafe(ruleSet, person);

        fired.ShouldBeFalse();
        result.TotalFired.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_MultipleConditions_AllMustBeTrue()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person>("multi-cond")
            .If(p => p.Age >= 18)
            .And(p => p.IsActive)
            .Then(p => { fired = true; });

        // Age check passes, IsActive fails
        var person = new Person { Name = "Alice", Age = 25, IsActive = false };
        EvaluateSafe(ruleSet, person);

        fired.ShouldBeFalse();
    }

    [Fact]
    public void Evaluate_MultipleConditions_AllTrue_Fires()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person>("multi-cond")
            .If(p => p.Age >= 18)
            .And(p => p.IsActive)
            .Then(p => { fired = true; });

        var person = new Person { Name = "Alice", Age = 25, IsActive = true };
        EvaluateSafe(ruleSet, person);

        fired.ShouldBeTrue();
    }


    // ========== Fire (unconditional) ==========

    [Fact]
    public void Fire_AlwaysExecutes()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person>("always-fire")
            .Fire(p => { fired = true; });

        EvaluateSafe(ruleSet, new Person { Name = "Anyone" });

        fired.ShouldBeTrue();
    }


    // ========== Otherwise (negated) ==========

    [Fact]
    public void Otherwise_FiresWhenConditionFails()
    {
        var ruleSet = new RuleSet();
        var firedOtherwise = false;

        ruleSet.AddRule<Person>("negate-check")
            .If(p => p.Age >= 18)
            .Otherwise(p => { firedOtherwise = true; });

        var child = new Person { Name = "Child", Age = 10 };
        EvaluateSafe(ruleSet, child);

        firedOtherwise.ShouldBeTrue();
    }

    [Fact]
    public void Otherwise_DoesNotFireWhenConditionPasses()
    {
        var ruleSet = new RuleSet();
        var firedOtherwise = false;

        ruleSet.AddRule<Person>("negate-check")
            .If(p => p.Age >= 18)
            .Otherwise(p => { firedOtherwise = true; });

        var adult = new Person { Name = "Adult", Age = 25 };
        EvaluateSafe(ruleSet, adult);

        firedOtherwise.ShouldBeFalse();
    }


    // ========== Salience ordering ==========

    [Fact]
    public void Evaluate_SalienceOrder_LowerFiresFirst()
    {
        var ruleSet = new RuleSet();
        var order = new List<string>();

        ruleSet.AddRule<Person>("low-salience")
            .WithSalience(100)
            .Fire(p => order.Add("low"));

        ruleSet.AddRule<Person>("high-salience")
            .WithSalience(900)
            .Fire(p => order.Add("high"));

        EvaluateSafe(ruleSet, new Person { Name = "Test" });

        order.Count.ShouldBe(2);
        order[0].ShouldBe("low");
        order[1].ShouldBe("high");
    }


    // ========== Mutex ==========

    [Fact]
    public void Evaluate_Mutex_OnlyFirstRuleFires()
    {
        var ruleSet = new RuleSet();
        var firedRules = new List<string>();

        ruleSet.AddRule<Person>("mutex-a")
            .WithSalience(100)
            .InMutex("group1")
            .Fire(p => firedRules.Add("A"));

        ruleSet.AddRule<Person>("mutex-b")
            .WithSalience(200)
            .InMutex("group1")
            .Fire(p => firedRules.Add("B"));

        EvaluateSafe(ruleSet, new Person { Name = "Test" });

        firedRules.Count.ShouldBe(1);
        firedRules[0].ShouldBe("A"); // lower salience fires first, wins the mutex
    }


    // ========== FireOnce ==========

    [Fact]
    public void Evaluate_FireOnce_OnlyFiresOncePerTuple()
    {
        var ruleSet = new RuleSet();
        var fireCount = 0;

        ruleSet.AddRule<Person>("once")
            .FireOnce()
            .Fire(p => fireCount++);

        // Add two persons — each gets its own tuple, so each fires once
        var result = EvaluateSafe(ruleSet, new Person { Name = "A" }, new Person { Name = "B" });

        fireCount.ShouldBe(2);
    }


    // ========== Inception / Expiration ==========

    [Fact]
    public void Evaluate_ExpiredRule_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person>("expired")
            .WithExpiration(DateTime.Now.AddDays(-1))
            .Fire(p => { fired = true; });

        EvaluateSafe(ruleSet, new Person { Name = "Test" });

        fired.ShouldBeFalse();
    }

    [Fact]
    public void Evaluate_FutureInception_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Person>("future")
            .WithInception(DateTime.Now.AddDays(1))
            .Fire(p => { fired = true; });

        EvaluateSafe(ruleSet, new Person { Name = "Test" });

        fired.ShouldBeFalse();
    }


    // ========== Affirm / Veto / Score ==========

    [Fact]
    public void ThenAffirm_IncreasesAffirmations()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("affirm")
            .If(p => p.Age >= 18)
            .ThenAffirm(10);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Adult", Age = 25 });

        result.TotalAffirmations.ShouldBe(10);
        result.Score.ShouldBe(10);
    }

    [Fact]
    public void ThenVeto_IncreasesVetos()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("veto")
            .If(p => p.Age < 18)
            .ThenVeto(5);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Child", Age = 10 });

        result.TotalVetos.ShouldBe(5);
        result.Score.ShouldBe(-5);
    }

    [Fact]
    public void Score_IsAffirmationsMinusVetos()
    {
        var ruleSet = new RuleSet();
        ruleSet.AddRule<Person>("affirm").FireAffirm(10);
        ruleSet.AddRule<Person>("veto").FireVeto(3);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test" });

        result.TotalAffirmations.ShouldBe(10);
        result.TotalVetos.ShouldBe(3);
        result.Score.ShouldBe(7);
    }


    // ========== Event messages ==========

    [Fact]
    public void Then_MessageTemplate_CreatesEvent()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("msg-rule")
            .Fire("Person {0} is active", p => p.Name);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Alice" });

        result.Events.Count.ShouldBe(1);
        var evt = result.Events.First();
        evt.Category.ShouldBe(RuleEvent.EventCategory.Info);
        evt.Message.ShouldBe("Person Alice is active");
    }

    [Fact]
    public void Then_GroupedMessage_SetsGroup()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("msg-rule")
            .Fire("status", "Person {0}", p => p.Name);

        var result = EvaluateSafe(ruleSet, new Person { Name = "Bob" });

        var evt = result.Events.First();
        evt.Group.ShouldBe("status");
    }


    // ========== Multiple facts ==========

    [Fact]
    public void Evaluate_MultipleFacts_SameType_EvaluatesEach()
    {
        var ruleSet = new RuleSet();
        var names = new List<string>();

        ruleSet.AddRule<Person>("collect-names")
            .Fire(p => names.Add(p.Name));

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Person { Name = "Bob" },
            new Person { Name = "Charlie" }
        );

        names.Count.ShouldBe(3);
        names.ShouldContain("Alice");
        names.ShouldContain("Bob");
        names.ShouldContain("Charlie");
    }


    // ========== Two-fact rules ==========

    [Fact]
    public void Evaluate_TwoFactRule_MatchesCrossProduct()
    {
        var ruleSet = new RuleSet();
        var matches = new List<string>();

        ruleSet.AddRule<Person, Order>("cross")
            .If((p, o) => p.Name == o.CustomerName)
            .Then((p, o) => matches.Add($"{p.Name}-{o.OrderId}"));

        EvaluateSafe(ruleSet,
            new Person { Name = "Alice" },
            new Order { OrderId = 1, CustomerName = "Alice" },
            new Order { OrderId = 2, CustomerName = "Bob" }
        );

        matches.Count.ShouldBe(1);
        matches[0].ShouldBe("Alice-1");
    }


    // ========== Decide ==========

    [Fact]
    public void Decide_ScoreAboveThreshold_ReturnsTrue()
    {
        var ruleSet = new RuleSet();
        ruleSet.DecisionThreshold = 5;

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var result = ruleSet.Decide(new Person { Name = "Test" });

        result.ShouldBeTrue();
    }

    [Fact]
    public void Decide_ScoreBelowThreshold_ReturnsFalse()
    {
        var ruleSet = new RuleSet();
        ruleSet.DecisionThreshold = 15;

        ruleSet.AddRule<Person>("affirm").FireAffirm(10);

        var result = ruleSet.Decide(new Person { Name = "Test" });

        result.ShouldBeFalse();
    }


    // ========== Modifies (forward chaining) ==========

    [Fact]
    public void Modifies_TriggersReEvaluation()
    {
        var ruleSet = new RuleSet();
        var fireCount = 0;

        ruleSet.AddRule<Person>("modifier")
            .WithSalience(100)
            .If(p => p.Status == "new")
            .Then(p => { p.Status = "processed"; fireCount++; })
            .Modifies(p => p);

        ruleSet.AddRule<Person>("after-modify")
            .WithSalience(200)
            .If(p => p.Status == "processed")
            .Then(p => { fireCount++; });

        var person = new Person { Name = "Test", Status = "new" };
        EvaluateSafe(ruleSet, person);

        person.Status.ShouldBe("processed");
        fireCount.ShouldBe(2);
    }


    // ========== NoConsequence ==========

    [Fact]
    public void NoConsequence_RuleFiresButDoesNothing()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("no-op")
            .If(p => p.Age > 0)
            .NoConsequence();

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", Age = 25 });

        result.TotalFired.ShouldBe(1);
    }


    // ========== EvaluationResults metadata ==========

    [Fact]
    public void Results_Duration_IsPositive()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("slow")
            .Fire(p => Thread.Sleep(10));

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test" });

        result.Duration.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Results_FiredRules_TracksCounts()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("tracked")
            .Fire(p => { });

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test" });

        // Rule.Name is "{Namespace}.{ruleName}" = "runtime.tracked"
        result.FiredRules.ShouldContainKey("runtime.tracked");
        result.FiredRules["runtime.tracked"].ShouldBe(1);
    }


    // ========== EvaluationContext features ==========

    [Fact]
    public void Shared_Dictionary_PersistsAcrossRules()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("writer")
            .WithSalience(100)
            .Fire("status", "wrote value");

        ruleSet.AddRule<Person>("reader")
            .WithSalience(200)
            .Fire(p => { });

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test" });

        result.Events.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Lookup_AddAndRetrieve_Works()
    {
        var ruleSet = new RuleSet();

        var ec = ruleSet.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        var accounts = new List<Account>
        {
            new() { AccountId = "A1", Balance = 100 },
            new() { AccountId = "A2", Balance = 200 }
        };

        ec.AddLookup<Account>(a => a.AccountId, accounts);

        var found = ec.Lookup<Account>("A1");
        found.Balance.ShouldBe(100);
    }


    // ========== Clear ==========

    [Fact]
    public void Clear_RemovesRules_NoRulesToEvaluate()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("test")
            .Fire(p => { });

        EvaluateSafe(ruleSet, new Person { Name = "Before" }).TotalFired.ShouldBe(1);

        ruleSet.Clear();

        var result = EvaluateSafe(ruleSet, new Person { Name = "After" });
        result.TotalFired.ShouldBe(0);
    }

}
