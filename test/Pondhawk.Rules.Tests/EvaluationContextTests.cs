using Pondhawk.Exceptions;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class EvaluationContextTests
{

    // ========== Defaults ==========

    [Fact]
    public void Constructor_SetsDefaults()
    {
        var ctx = new EvaluationContext();

        ctx.ThrowValidationException.ShouldBeTrue();
        ctx.ThrowNoRulesException.ShouldBeTrue();
        ctx.MaxEvaluations.ShouldBe(500000);
        ctx.MaxDuration.ShouldBe(10_000);
        ctx.MaxViolations.ShouldBe(int.MaxValue);
        ctx.Description.ShouldBe("");
        ctx.Results.ShouldNotBeNull();
    }


    // ========== AddFacts ==========

    [Fact]
    public void AddFacts_PopulatesSpace()
    {
        var ctx = new EvaluationContext();
        ctx.AddFacts(new Person { Name = "A" }, new Order { OrderId = 1 });

        ctx.Space.TypeCount.ShouldBe(2);
    }


    // ========== Lookup ==========

    [Fact]
    public void AddLookup_ByType_LookupReturnsCorrectMember()
    {
        var ctx = new EvaluationContext();

        var accounts = new List<Account>
        {
            new() { AccountId = "A1", Balance = 100m },
            new() { AccountId = "A2", Balance = 200m },
        };

        ctx.AddLookup<Account>(a => a.AccountId, accounts);

        var result = ctx.Lookup<Account>("A2");
        result.Balance.ShouldBe(200m);
    }

    [Fact]
    public void AddLookup_ByName_LookupByNameWorks()
    {
        var ctx = new EvaluationContext();

        var accounts = new List<Account>
        {
            new() { AccountId = "A1", Balance = 100m },
        };

        ctx.AddLookup("accounts", a => (object)a.AccountId, accounts.Cast<Account>().Select(a => (Account)a).ToList());

        // For named lookup, we need to use the named variant
        // Actually the AddLookup<TMember> uses FullName as the name
        // Let's use the direct dictionary overload instead
        var table = new Dictionary<object, object> { ["A1"] = accounts[0] };
        ctx.AddLookup("custom-accounts", table);

        var found = ctx.Lookup<Account>("custom-accounts", "A1");
        found.AccountId.ShouldBe("A1");
    }

    [Fact]
    public void Lookup_MissingTable_Throws()
    {
        var ctx = new EvaluationContext();

        Should.Throw<InvalidOperationException>(() => ctx.Lookup<Account>("does-not-exist", "key"));
    }

    [Fact]
    public void Lookup_MissingKey_Throws()
    {
        var ctx = new EvaluationContext();
        var table = new Dictionary<object, object> { ["A1"] = new Account { AccountId = "A1" } };
        ctx.AddLookup("test", table);

        Should.Throw<InvalidOperationException>(() => ctx.Lookup<Account>("test", "missing"));
    }


    // ========== Shared dictionary ==========

    [Fact]
    public void Shared_IsSameAsResults_Shared()
    {
        var ctx = new EvaluationContext();

        ctx.Shared["key"] = "value";

        ctx.Results.Shared["key"].ShouldBe("value");
    }


    // ========== EvaluationResults ==========

    [Fact]
    public void Results_Score_IsAffirmationsMinusVetos()
    {
        var results = new EvaluationResults();

        results.Affirm(10);
        results.Veto(3);

        results.TotalAffirmations.ShouldBe(10);
        results.TotalVetos.ShouldBe(3);
        results.Score.ShouldBe(7);
    }

    [Fact]
    public void Results_HasViolations_NoEvents_ReturnsFalse()
    {
        var results = new EvaluationResults();

        results.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void Results_HasViolations_WithViolation_ReturnsTrue()
    {
        var results = new EvaluationResults();
        results.Events.Add(new EventDetail
        {
            Category = EventDetail.EventCategory.Violation,
            Explanation = "test violation",
            Group = "test",
            RuleName = "rule1",
            Source = "source"
        });

        results.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void Results_HasViolations_WithInfoOnly_ReturnsFalse()
    {
        var results = new EvaluationResults();
        results.Events.Add(new EventDetail
        {
            Category = EventDetail.EventCategory.Info,
            Explanation = "info event",
            Group = "test",
            RuleName = "rule1",
            Source = "source"
        });

        results.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void Results_MutexWinners_TracksMutexOutcomes()
    {
        var results = new EvaluationResults();

        results.MutexWinners["group1"] = "rule-a";

        results.MutexWinners["group1"].ShouldBe("rule-a");
    }

}
