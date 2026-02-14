using Pondhawk.Exceptions;
using Pondhawk.Rules.Factory;
using Pondhawk.Rules.Validators;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class ValidationAdvancedTests
{

    private static EvaluationResults EvaluateSafe(RuleSet ruleSet, params object[] facts)
    {
        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.AddAllFacts(facts);
        return ruleSet.Evaluate(ctx);
    }


    // ========== When scoping ==========

    [Fact]
    public void When_PredicateTrue_ValidatesNormally()
    {
        var ruleSet = new RuleSet();

        var vr = ruleSet.AddValidation<Person>("when-true");
        vr.When(p => p.IsActive)
            .Assert<string>(p => p.Email)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Email required for active users");

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", IsActive = true, Email = "" });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void When_PredicateFalse_SkipsValidation()
    {
        var ruleSet = new RuleSet();

        var vr = ruleSet.AddValidation<Person>("when-false");
        vr.When(p => p.IsActive)
            .Assert<string>(p => p.Email)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Email required for active users");

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", IsActive = false, Email = "" });

        result.HasViolations.ShouldBeFalse();
    }


    // ========== And predicate ==========

    [Fact]
    public void And_AddsPredicate_BothMustPass()
    {
        var ruleSet = new RuleSet();

        var vr = ruleSet.AddValidation<Person>("and-pred");
        vr.When(p => p.IsActive)
            .And(p => p.Age >= 18)
            .Assert<string>(p => p.Email)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Email required for active adults");

        // Active but minor — And(p.Age >= 18) fails, validation skipped
        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", IsActive = true, Age = 10, Email = "" });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void And_BothPredicatesPass_ValidationRuns()
    {
        var ruleSet = new RuleSet();

        var vr = ruleSet.AddValidation<Person>("and-pass");
        vr.When(p => p.IsActive)
            .And(p => p.Age >= 18)
            .Assert<string>(p => p.Email)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Email required for active adults");

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", IsActive = true, Age = 25, Email = "" });

        result.HasViolations.ShouldBeTrue();
    }


    // ========== Mutex ==========

    [Fact]
    public void Mutex_OnlyFirstMatchingValidationFires()
    {
        var ruleSet = new RuleSet();

        var v1 = ruleSet.AddValidation<Person>("mutex-a");
        v1.InMutex("val-group")
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name is required");

        var v2 = ruleSet.AddValidation<Person>("mutex-b");
        v2.InMutex("val-group")
            .Assert<string>(p => p.Email)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Email is required");

        // Both name and email are empty, but only first in mutex group fires
        var result = EvaluateSafe(ruleSet, new Person { Name = "", Email = "" });

        result.Events.Count.ShouldBe(1);
    }


    // ========== WithSalience ==========

    [Fact]
    public void WithSalience_AdjustsEffectiveSalience()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("sal-b")
            .WithSalience(200)
            .Assert<string>(p => p.Email)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Email required");

        ruleSet.AddValidation<Person>("sal-a")
            .WithSalience(100)
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        // Both fail, both produce violations
        var result = EvaluateSafe(ruleSet, new Person { Name = "", Email = "" });

        result.Events.Count.ShouldBe(2);
        result.HasViolations.ShouldBeTrue();
    }


    // ========== AssertOver — enumerable validators ==========

    [Fact]
    public void AssertOver_Required_FailsForEmptyCollection()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("addr-required")
            .AssertOver<Address>(p => p.Addresses)
            .Required();

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", Addresses = [] });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void AssertOver_Required_PassesForNonEmptyCollection()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("addr-required")
            .AssertOver<Address>(p => p.Addresses)
            .Required();

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Test", Addresses = [new Address { Street = "1st" }] });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_IsEmpty_PassesForEmptyCollection()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("addr-empty")
            .AssertOver<Address>(p => p.Addresses)
            .IsEmpty();

        var result = EvaluateSafe(ruleSet, new Person { Name = "Test", Addresses = [] });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_IsEmpty_FailsForNonEmptyCollection()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("addr-empty")
            .AssertOver<Address>(p => p.Addresses)
            .IsEmpty();

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Test", Addresses = [new Address { Street = "1st" }] });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void AssertOver_IsNotEmpty_PassesForNonEmpty()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("addr-notempty")
            .AssertOver<Address>(p => p.Addresses)
            .IsNotEmpty();

        var result = EvaluateSafe(ruleSet,
            new Person { Name = "Test", Addresses = [new Address { Street = "1st" }] });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_Has_SucceedsWhenItemMatches()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("has-addr")
            .AssertOver<Address>(p => p.Addresses)
            .Has(a => a.City == "Springfield");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses =
                [
                    new Address { Street = "1st", City = "Other" },
                    new Address { Street = "2nd", City = "Springfield" }
                ]
            });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_Has_FailsWhenNoItemMatches()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("has-addr")
            .AssertOver<Address>(p => p.Addresses)
            .Has(a => a.City == "Springfield");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses = [new Address { Street = "1st", City = "Other" }]
            });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void AssertOver_HasNone_SucceedsWhenNoItemMatches()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("hasnone-addr")
            .AssertOver<Address>(p => p.Addresses)
            .HasNone(a => a.City == "BadCity");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses = [new Address { Street = "1st", City = "GoodCity" }]
            });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_HasNone_FailsWhenItemMatches()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("hasnone-addr")
            .AssertOver<Address>(p => p.Addresses)
            .HasNone(a => a.City == "BadCity");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses = [new Address { Street = "1st", City = "BadCity" }]
            });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void AssertOver_HasExactly_MatchesCount()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("exact-addr")
            .AssertOver<Address>(p => p.Addresses)
            .HasExactly(a => a.State == "TX", 2);

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses =
                [
                    new Address { Street = "1st", State = "TX" },
                    new Address { Street = "2nd", State = "TX" },
                    new Address { Street = "3rd", State = "CA" }
                ]
            });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_HasExactly_FailsWhenCountDoesNotMatch()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("exact-addr")
            .AssertOver<Address>(p => p.Addresses)
            .HasExactly(a => a.State == "TX", 2);

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses = [new Address { Street = "1st", State = "TX" }]
            });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void AssertOver_HasOnlyOne_SucceedsWithExactlyOne()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("only-one")
            .AssertOver<Address>(p => p.Addresses)
            .HasOnlyOne(a => a.State == "TX");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses =
                [
                    new Address { Street = "1st", State = "TX" },
                    new Address { Street = "2nd", State = "CA" }
                ]
            });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_HasOnlyOne_FailsWithMultiple()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("only-one")
            .AssertOver<Address>(p => p.Addresses)
            .HasOnlyOne(a => a.State == "TX");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses =
                [
                    new Address { Street = "1st", State = "TX" },
                    new Address { Street = "2nd", State = "TX" }
                ]
            });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void AssertOver_HasAtMostOne_SucceedsWithZero()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("atmost-one")
            .AssertOver<Address>(p => p.Addresses)
            .HasAtMostOne(a => a.State == "TX");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses = [new Address { Street = "1st", State = "CA" }]
            });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_HasAtMostOne_SucceedsWithOne()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("atmost-one")
            .AssertOver<Address>(p => p.Addresses)
            .HasAtMostOne(a => a.State == "TX");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses = [new Address { Street = "1st", State = "TX" }]
            });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_HasAtMostOne_FailsWithTwo()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("atmost-one")
            .AssertOver<Address>(p => p.Addresses)
            .HasAtMostOne(a => a.State == "TX");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses =
                [
                    new Address { Street = "1st", State = "TX" },
                    new Address { Street = "2nd", State = "TX" }
                ]
            });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void AssertOver_HasAtLeast_SucceedsWhenCountMet()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("atleast")
            .AssertOver<Address>(p => p.Addresses)
            .HasAtLeast(a => a.State == "TX", 2);

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses =
                [
                    new Address { Street = "1st", State = "TX" },
                    new Address { Street = "2nd", State = "TX" },
                    new Address { Street = "3rd", State = "CA" }
                ]
            });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_HasAtLeast_FailsWhenCountNotMet()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("atleast")
            .AssertOver<Address>(p => p.Addresses)
            .HasAtLeast(a => a.State == "TX", 3);

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses = [new Address { Street = "1st", State = "TX" }]
            });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void AssertOver_HasAtMost_SucceedsWhenCountNotExceeded()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("atmost")
            .AssertOver<Address>(p => p.Addresses)
            .HasAtMost(a => a.State == "TX", 2);

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses =
                [
                    new Address { Street = "1st", State = "TX" },
                    new Address { Street = "2nd", State = "CA" }
                ]
            });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void AssertOver_HasAtMost_FailsWhenCountExceeded()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("atmost")
            .AssertOver<Address>(p => p.Addresses)
            .HasAtMost(a => a.State == "TX", 1);

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses =
                [
                    new Address { Street = "1st", State = "TX" },
                    new Address { Street = "2nd", State = "TX" }
                ]
            });

        result.HasViolations.ShouldBeTrue();
    }


    // ========== AssertOver IsNot ==========

    [Fact]
    public void AssertOver_IsNot_NegatesCondition()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("isnot")
            .AssertOver<Address>(p => p.Addresses)
            .IsNot((p, addrs) => addrs.Any(a => a.City == "Banned"))
            .Otherwise("Contains banned city");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses = [new Address { Street = "1st", City = "Banned" }]
            });

        result.HasViolations.ShouldBeTrue();
    }

    [Fact]
    public void AssertOver_IsNot_PassesWhenConditionFalse()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("isnot-pass")
            .AssertOver<Address>(p => p.Addresses)
            .IsNot((p, addrs) => addrs.Any(a => a.City == "Banned"))
            .Otherwise("Contains banned city");

        var result = EvaluateSafe(ruleSet,
            new Person
            {
                Name = "Test",
                Addresses = [new Address { Street = "1st", City = "Allowed" }]
            });

        result.HasViolations.ShouldBeFalse();
    }


    // ========== Validation Cascade ==========

    [Fact]
    public void ValidationRule_Cascade_InsertsChildFact()
    {
        var ruleSet = new RuleSet();
        var addressValidated = false;

        ruleSet.AddValidation<Person>("cascade-val")
            .Cascade(p => p.Addresses.FirstOrDefault());

        ruleSet.AddRule<Address>("addr-check")
            .Fire(a => { addressValidated = true; });

        var person = new Person
        {
            Name = "Test",
            Addresses = [new Address { Street = "1st", City = "Test" }]
        };

        EvaluateSafe(ruleSet, person);

        addressValidated.ShouldBeTrue();
    }

    [Fact]
    public void ValidationRule_CascadeAll_InsertsMultipleChildren()
    {
        var ruleSet = new RuleSet();
        var processedCities = new List<string>();

        ruleSet.AddValidation<Person>("cascade-all-val")
            .CascadeAll(p => p.Addresses);

        ruleSet.AddRule<Address>("addr-process")
            .Fire(a => processedCities.Add(a.City));

        var person = new Person
        {
            Name = "Test",
            Addresses =
            [
                new Address { Street = "1st", City = "Alpha" },
                new Address { Street = "2nd", City = "Beta" }
            ]
        };

        EvaluateSafe(ruleSet, person);

        processedCities.Count.ShouldBe(2);
    }


    // ========== Inception/Expiration on validation ==========

    [Fact]
    public void ValidationRule_FutureInception_SkipsValidation()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("future-val")
            .WithInception(DateTime.Now.AddDays(1))
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        var result = EvaluateSafe(ruleSet, new Person { Name = "" });

        result.HasViolations.ShouldBeFalse();
    }

    [Fact]
    public void ValidationRule_PastExpiration_SkipsValidation()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddValidation<Person>("expired-val")
            .WithExpiration(DateTime.Now.AddDays(-1))
            .Assert<string>(p => p.Name)
            .Is((p, v) => !string.IsNullOrWhiteSpace(v))
            .Otherwise("Name required");

        var result = EvaluateSafe(ruleSet, new Person { Name = "" });

        result.HasViolations.ShouldBeFalse();
    }

}
