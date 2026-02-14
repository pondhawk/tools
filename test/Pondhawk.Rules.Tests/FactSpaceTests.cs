using Pondhawk.Rules.Evaluation;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class FactSpaceTests
{

    // ========== Add facts ==========

    [Fact]
    public void Add_SingleFact_TracksTypeAndPosition()
    {
        var space = new FactSpace();

        space.Add("hello");

        space.TypeCount.ShouldBe(1);
        space.Schema[0].FactType.ShouldBe(typeof(string));
        space.Schema[0].Members.Count.ShouldBe(1);
    }

    [Fact]
    public void Add_MultipleFacts_SameType_SingleSchema()
    {
        var space = new FactSpace();

        space.Add("one", "two", "three");

        space.TypeCount.ShouldBe(1);
        space.Schema[0].FactType.ShouldBe(typeof(string));
        space.Schema[0].Members.Count.ShouldBe(3);
    }

    [Fact]
    public void Add_MultipleFacts_DifferentTypes_MultipleSchemas()
    {
        var space = new FactSpace();
        var person = new Person { Name = "Alice" };
        var order = new Order { OrderId = 1 };

        space.Add(person, order);

        space.TypeCount.ShouldBe(2);
    }

    [Fact]
    public void Add_DuplicateFact_IgnoredByGuard()
    {
        var space = new FactSpace();
        var person = new Person { Name = "Alice" };

        space.Add(person, person);

        space.TypeCount.ShouldBe(1);
        space.Schema[0].Members.Count.ShouldBe(1);
    }

    [Fact]
    public void Add_TwoDifferentInstances_BothAdded()
    {
        var space = new FactSpace();
        var p1 = new Person { Name = "Alice" };
        var p2 = new Person { Name = "Bob" };

        space.Add(p1, p2);

        space.TypeCount.ShouldBe(1);
        space.Schema[0].Members.Count.ShouldBe(2);
    }

    [Fact]
    public void AddAll_EnumerableOfFacts_Works()
    {
        var space = new FactSpace();
        var facts = new object[] { new Person { Name = "A" }, new Order { OrderId = 1 } };

        space.AddAll(facts);

        space.TypeCount.ShouldBe(2);
    }


    // ========== GetTuple ==========

    [Fact]
    public void GetTuple_ValidSelector_ReturnsFact()
    {
        var space = new FactSpace();
        var person = new Person { Name = "Alice" };

        space.Add(person);

        // After adding, position 1 should map to identity 0 (first fact)
        var tuple = space.GetTuple([1]);

        tuple.Length.ShouldBe(1);
        tuple[0].ShouldBe(person);
    }

    [Fact]
    public void GetTuple_MultipleFacts_ReturnsBoth()
    {
        var space = new FactSpace();
        var p1 = new Person { Name = "Alice" };
        var p2 = new Order { OrderId = 1 };

        space.Add(p1, p2);

        var tuple = space.GetTuple([1, 2]);

        tuple.Length.ShouldBe(2);
        tuple[0].ShouldBe(p1);
        tuple[1].ShouldBe(p2);
    }

    [Fact]
    public void GetTuple_InvalidSelector_ReturnsEmpty()
    {
        var space = new FactSpace();
        var person = new Person { Name = "Alice" };

        space.Add(person);

        var tuple = space.GetTuple([999]);

        tuple.ShouldBeNull();
    }


    // ========== GetFactTypes ==========

    [Fact]
    public void GetFactTypes_ReturnsCorrectTypes()
    {
        var space = new FactSpace();

        space.Add(new Person { Name = "A" });
        space.Add(new Order { OrderId = 1 });

        var types = space.GetFactTypes([0, 1]);

        types.Length.ShouldBe(2);
        types[0].ShouldBe(typeof(Person));
        types[1].ShouldBe(typeof(Order));
    }


    // ========== InsertFact / ModifyFact / RetractFact ==========

    [Fact]
    public void InsertFact_AddsToSpace()
    {
        var space = new FactSpace();

        space.InsertFact(new Person { Name = "Inserted" });

        space.TypeCount.ShouldBe(1);
    }

    [Fact]
    public void RetractFact_RemovesSelectorMapping()
    {
        var space = new FactSpace();
        var person = new Person { Name = "Alice" };

        space.Add(person);

        // Selector 1 should be valid
        space.GetTuple([1]).Length.ShouldBe(1);

        // Retract it
        space.RetractFact(1);

        // Now GetTuple should return null since selector 1 is removed
        space.GetTuple([1]).ShouldBeNull();
    }

    [Fact]
    public void ModifyFact_ReassignsSelector()
    {
        var space = new FactSpace();
        var person = new Person { Name = "Alice" };

        space.Add(person);

        // Position 1 maps to identity 0
        space.GetTuple([1])[0].ShouldBe(person);

        // Modify: removes old selector (1), creates new one (2)
        space.ModifyFact(1);

        // Old selector should no longer work
        space.GetTuple([1]).ShouldBeNull();

        // New selector (2) should point to the same fact
        space.GetTuple([2])[0].ShouldBe(person);
    }


    // ========== GetIdentityFromSelector ==========

    [Fact]
    public void GetIdentityFromSelector_MapsCorrectly()
    {
        var space = new FactSpace();
        var person = new Person { Name = "Alice" };

        space.Add(person);

        var identity = space.GetIdentityFromSelector([1]);

        identity.Length.ShouldBe(1);
        identity[0].ShouldBe(0); // first fact has identity 0
    }

}
