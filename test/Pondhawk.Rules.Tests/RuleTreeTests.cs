using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Tree;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class RuleTreeTests
{

    // ========== Add and FindRules ==========

    [Fact]
    public void Add_SingleRule_FindRules_ReturnsSameRule()
    {
        var tree = new RuleTree();
        var rule = new Rule<Person>("test", "rule1")
            .If(p => p.Age > 18)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule]);

        var found = tree.FindRules([typeof(Person)], []);

        found.ShouldNotBeNull();
        found.ShouldContain(rule);
    }

    [Fact]
    public void FindRules_NoMatchingType_ReturnsEmpty()
    {
        var tree = new RuleTree();
        var rule = new Rule<Person>("test", "rule1")
            .If(p => p.Age > 18)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule]);

        var found = tree.FindRules([typeof(Order)], []);

        found.ShouldBeEmpty();
    }

    [Fact]
    public void Add_MultipleRules_SameType_AllFound()
    {
        var tree = new RuleTree();
        var rule1 = new Rule<Person>("test", "rule1")
            .If(p => p.Age > 18)
            .Then(p => { });
        var rule2 = new Rule<Person>("test", "rule2")
            .If(p => p.Age < 65)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule1, rule2]);

        var found = tree.FindRules([typeof(Person)], []).ToList();

        found.Count.ShouldBe(2);
        found.ShouldContain(rule1);
        found.ShouldContain(rule2);
    }

    [Fact]
    public void Add_TwoFactRule_FindRules_Works()
    {
        var tree = new RuleTree();
        var rule = new Rule<Person, Order>("test", "cross-rule")
            .If((p, o) => p.Name == o.CustomerName)
            .Then((p, o) => { });

        tree.Add([typeof(Person), typeof(Order)], [rule]);

        var found = tree.FindRules([typeof(Person), typeof(Order)], []);

        found.ShouldContain(rule);
    }


    // ========== Namespace filtering ==========

    [Fact]
    public void FindRules_WithNamespace_ReturnsOnlyMatchingNamespace()
    {
        var tree = new RuleTree();
        var rule1 = new Rule<Person>("namespace1", "rule1")
            .If(p => true)
            .Then(p => { });
        var rule2 = new Rule<Person>("namespace2", "rule2")
            .If(p => true)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule1, rule2]);

        var found = tree.FindRules([typeof(Person)], ["namespace1"]).ToList();

        found.Count.ShouldBe(1);
        found.ShouldContain(rule1);
    }

    [Fact]
    public void FindRules_EmptyNamespaces_ReturnsAll()
    {
        var tree = new RuleTree();
        var rule1 = new Rule<Person>("ns1", "rule1")
            .If(p => true)
            .Then(p => { });
        var rule2 = new Rule<Person>("ns2", "rule2")
            .If(p => true)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule1, rule2]);

        var found = tree.FindRules([typeof(Person)], []).ToList();

        found.Count.ShouldBe(2);
    }


    // ========== Clear ==========

    [Fact]
    public void Clear_RemovesAllRules()
    {
        var tree = new RuleTree();
        var rule = new Rule<Person>("test", "rule1")
            .If(p => true)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule]);
        tree.FindRules([typeof(Person)], []).ShouldNotBeEmpty();

        tree.Clear();

        tree.FindRules([typeof(Person)], []).ShouldBeEmpty();
    }


    // ========== Polymorphic matching ==========

    [Fact]
    public void FindRules_BaseType_MatchesDerivedTypeFacts()
    {
        var tree = new RuleTree();

        // Add a rule for object (base type of everything)
        var rule = new Rule<object>("test", "object-rule")
            .Fire(o => { });

        tree.Add([typeof(object)], [rule]);

        // FindRules with Person should match rules registered for object
        // because IsAssignableFrom is used
        var found = tree.FindRules([typeof(Person)], []).ToList();

        found.ShouldContain(rule);
    }


    // ========== Cache invalidation ==========

    [Fact]
    public void FindRules_CacheInvalidated_WhenNewRuleAddedForSameArity()
    {
        var tree = new RuleTree();
        var rule1 = new Rule<Person>("test", "rule1")
            .If(p => true)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule1]);

        // Warm the cache
        tree.FindRules([typeof(Person)], []).Count.ShouldBe(1);

        // Add a rule for a base type — should be found for Person queries
        var rule2 = new Rule<object>("test", "object-rule")
            .Fire(o => { });
        tree.Add([typeof(object)], [rule2]);

        // The cache must have been invalidated so the new rule is visible
        var found = tree.FindRules([typeof(Person)], []).ToList();
        found.Count.ShouldBe(2);
        found.ShouldContain(rule1);
        found.ShouldContain(rule2);
    }

    [Fact]
    public void FindRules_CacheInvalidated_AfterClear()
    {
        var tree = new RuleTree();
        var rule = new Rule<Person>("test", "rule1")
            .If(p => true)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule]);

        // Warm the cache
        tree.FindRules([typeof(Person)], []).ShouldNotBeEmpty();

        tree.Clear();

        // After clear the cache must be gone
        tree.FindRules([typeof(Person)], []).ShouldBeEmpty();

        // Re-add and verify new lookups work
        var rule2 = new Rule<Person>("test", "rule2")
            .If(p => true)
            .Then(p => { });
        tree.Add([typeof(Person)], [rule2]);

        var found = tree.FindRules([typeof(Person)], []);
        found.Count.ShouldBe(1);
        found.ShouldContain(rule2);
    }

    [Fact]
    public void FindRules_CacheInvalidated_WhenBranchAddedForMultiArity()
    {
        var tree = new RuleTree();
        var rule1 = new Rule<Person, Order>("test", "rule1")
            .If((p, o) => true)
            .Then((p, o) => { });

        tree.Add([typeof(Person), typeof(Order)], [rule1]);

        // Warm the cache
        tree.FindRules([typeof(Person), typeof(Order)], []).Count.ShouldBe(1);

        // Add a second 2-arity rule with a base type at depth 1
        var rule2 = new Rule<Person, object>("test", "rule2")
            .If((p, o) => true)
            .Then((p, o) => { });
        tree.Add([typeof(Person), typeof(object)], [rule2]);

        // Order is assignable to object, so both rules should match
        var found = tree.FindRules([typeof(Person), typeof(Order)], []).ToList();
        found.Count.ShouldBe(2);
        found.ShouldContain(rule1);
        found.ShouldContain(rule2);
    }


    // ========== HasRules ==========

    [Fact]
    public void HasRules_ReturnsTrueForMatchingType()
    {
        var tree = new RuleTree();
        var rule = new Rule<Person>("test", "rule1")
            .If(p => true)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule]);

        tree.HasRules([typeof(Person)]).ShouldBeTrue();
    }

    [Fact]
    public void HasRules_ReturnsFalseForUnregisteredType()
    {
        var tree = new RuleTree();
        var rule = new Rule<Person>("test", "rule1")
            .If(p => true)
            .Then(p => { });

        tree.Add([typeof(Person)], [rule]);

        tree.HasRules([typeof(Order)]).ShouldBeFalse();
    }

    [Fact]
    public void HasRules_PolymorphicMatch_ReturnsTrueForDerivedType()
    {
        var tree = new RuleTree();
        var rule = new Rule<object>("test", "object-rule")
            .Fire(o => { });

        tree.Add([typeof(object)], [rule]);

        tree.HasRules([typeof(Person)]).ShouldBeTrue();
    }

}
