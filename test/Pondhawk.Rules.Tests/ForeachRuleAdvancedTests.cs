using Pondhawk.Rules.Factory;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class ForeachRuleAdvancedTests
{

    private static EvaluationResults EvaluateSafe(RuleSet ruleSet, params object[] facts)
    {
        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowValidationException = false;
        ctx.ThrowNoRulesException = false;
        ctx.AddAllFacts(facts);
        return ruleSet.Evaluate(ctx);
    }


    // ========== Otherwise ==========

    [Fact]
    public void Otherwise_FiresForItemsThatFailCondition()
    {
        var ruleSet = new RuleSet();
        var otherwiseItems = new List<string>();

        ruleSet.AddRule<Order, OrderItem>("otherwise", o => o.Items)
            .If(item => item.Quantity > 0)
            .Otherwise(item => otherwiseItems.Add(item.Product));

        var order = new Order
        {
            OrderId = 1,
            Items =
            [
                new OrderItem { Product = "Widget", Quantity = 5 },
                new OrderItem { Product = "Gadget", Quantity = 0 }
            ]
        };

        EvaluateSafe(ruleSet, order);

        otherwiseItems.Count.ShouldBe(1);
        otherwiseItems[0].ShouldBe("Gadget");
    }

    [Fact]
    public void Otherwise_DoesNotFireWhenAllMatch()
    {
        var ruleSet = new RuleSet();
        var otherwiseCount = 0;

        ruleSet.AddRule<Order, OrderItem>("otherwise", o => o.Items)
            .If(item => item.Quantity > 0)
            .Otherwise(item => otherwiseCount++);

        var order = new Order
        {
            OrderId = 1,
            Items =
            [
                new OrderItem { Product = "Widget", Quantity = 5 },
                new OrderItem { Product = "Gadget", Quantity = 3 }
            ]
        };

        EvaluateSafe(ruleSet, order);

        otherwiseCount.ShouldBe(0);
    }


    // ========== Otherwise message templates ==========

    [Fact]
    public void Otherwise_MessageTemplate()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("oth-msg", o => o.Items)
            .If(item => item.Quantity > 0)
            .Otherwise("{0} has no quantity", item => item.Product);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Gadget", Quantity = 0 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.Count.ShouldBe(1);
        result.Events.First().Message.ShouldBe("Gadget has no quantity");
    }

    [Fact]
    public void Otherwise_GroupedMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("oth-grp", o => o.Items)
            .If(item => item.Quantity > 0)
            .Otherwise("items", "{0} missing", item => item.Product);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Gadget", Quantity = 0 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.First().Group.ShouldBe("items");
    }

    [Fact]
    public void Otherwise_CategoryMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("oth-cat", o => o.Items)
            .If(item => item.Quantity > 0)
            .Otherwise(RuleEvent.EventCategory.Violation, "items", "{0} invalid", item => item.Product);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Gadget", Quantity = 0 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.First().Category.ShouldBe(RuleEvent.EventCategory.Violation);
    }


    // ========== OtherwiseAffirm / OtherwiseVeto ==========

    [Fact]
    public void OtherwiseAffirm_ScoringForFailedItems()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("oa", o => o.Items)
            .If(item => item.Quantity > 100)
            .OtherwiseAffirm(5);

        var order = new Order
        {
            OrderId = 1,
            Items =
            [
                new OrderItem { Product = "Widget", Quantity = 1 },
                new OrderItem { Product = "Gadget", Quantity = 2 }
            ]
        };

        var result = EvaluateSafe(ruleSet, order);

        // Both items fail condition (Quantity not > 100), so each triggers OtherwiseAffirm
        result.TotalAffirmations.ShouldBe(10);
    }

    [Fact]
    public void OtherwiseVeto_ScoringForFailedItems()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("ov", o => o.Items)
            .If(item => item.Quantity > 100)
            .OtherwiseVeto(3);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 1 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.TotalVetos.ShouldBe(3);
    }


    // ========== ThenAffirm / ThenVeto ==========

    [Fact]
    public void ThenAffirm_ScoringForMatchedItems()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("ta", o => o.Items)
            .If(item => item.Quantity > 0)
            .ThenAffirm(7);

        var order = new Order
        {
            OrderId = 1,
            Items =
            [
                new OrderItem { Product = "Widget", Quantity = 5 },
                new OrderItem { Product = "Gadget", Quantity = 3 }
            ]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.TotalAffirmations.ShouldBe(14);
    }

    [Fact]
    public void ThenVeto_ScoringForMatchedItems()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("tv", o => o.Items)
            .If(item => item.Price <= 0)
            .ThenVeto(4);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Free", Quantity = 1, Price = 0 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.TotalVetos.ShouldBe(4);
    }


    // ========== FireAffirm / FireVeto ==========

    [Fact]
    public void FireAffirm_UnconditionalScoring()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("fa", o => o.Items)
            .FireAffirm(10);

        var order = new Order
        {
            OrderId = 1,
            Items =
            [
                new OrderItem { Product = "A", Quantity = 1 },
                new OrderItem { Product = "B", Quantity = 2 }
            ]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.TotalAffirmations.ShouldBe(20);
    }

    [Fact]
    public void FireVeto_UnconditionalScoring()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("fv", o => o.Items)
            .FireVeto(6);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "A", Quantity = 1 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.TotalVetos.ShouldBe(6);
    }


    // ========== Then message templates ==========

    [Fact]
    public void Then_MessageTemplate()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("msg", o => o.Items)
            .If(item => item.Quantity > 0)
            .Then("Processed {0}", item => item.Product);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 5 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.First().Message.ShouldBe("Processed Widget");
    }

    [Fact]
    public void Then_GroupedMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("grp", o => o.Items)
            .If(item => item.Quantity > 0)
            .Then("processing", "Item {0}", item => item.Product);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 5 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.First().Group.ShouldBe("processing");
    }

    [Fact]
    public void Then_CategoryMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("cat", o => o.Items)
            .If(item => item.Quantity > 0)
            .Then(RuleEvent.EventCategory.Warning, "trace", "Item {0}", item => item.Product);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 5 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.First().Category.ShouldBe(RuleEvent.EventCategory.Warning);
    }


    // ========== Fire message templates ==========

    [Fact]
    public void Fire_MessageTemplate()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("fire-msg", o => o.Items)
            .Fire("Item: {0}", item => item.Product);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 5 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.First().Message.ShouldBe("Item: Widget");
    }

    [Fact]
    public void Fire_GroupedMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("fire-grp", o => o.Items)
            .Fire("items", "Item: {0}", item => item.Product);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 5 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.First().Group.ShouldBe("items");
    }

    [Fact]
    public void Fire_CategoryMessage()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("fire-cat", o => o.Items)
            .Fire(RuleEvent.EventCategory.Violation, "bad", "Bad: {0}", item => item.Product);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 5 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.First().Category.ShouldBe(RuleEvent.EventCategory.Violation);
    }


    // ========== Then null parameter ==========

    [Fact]
    public void Then_NullParameter_RendersAsNull()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("null", o => o.Items)
            .Fire("Value: {0}", item => (object)null);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 1 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.Events.First().Message.ShouldBe("Value: null");
    }


    // ========== WithSalience ==========

    [Fact]
    public void WithSalience_AdjustsOrdering()
    {
        var ruleSet = new RuleSet();
        var order = new List<string>();

        ruleSet.AddRule<Order, OrderItem>("second", o => o.Items)
            .WithSalience(200)
            .Fire(item => order.Add("second-" + item.Product));

        ruleSet.AddRule<Order, OrderItem>("first", o => o.Items)
            .WithSalience(100)
            .Fire(item => order.Add("first-" + item.Product));

        var o1 = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "A", Quantity = 1 }]
        };

        EvaluateSafe(ruleSet, o1);

        order[0].ShouldBe("first-A");
    }


    // ========== WithInception / WithExpiration ==========

    [Fact]
    public void WithInception_FutureInception_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Order, OrderItem>("future", o => o.Items)
            .WithInception(DateTime.Now.AddDays(1))
            .Fire(item => { fired = true; });

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 1 }]
        };

        EvaluateSafe(ruleSet, order);

        fired.ShouldBeFalse();
    }

    [Fact]
    public void WithExpiration_PastExpiration_DoesNotFire()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Order, OrderItem>("expired", o => o.Items)
            .WithExpiration(DateTime.Now.AddDays(-1))
            .Fire(item => { fired = true; });

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 1 }]
        };

        EvaluateSafe(ruleSet, order);

        fired.ShouldBeFalse();
    }

    [Fact]
    public void WithInceptionAndExpiration_WithinWindow_Fires()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Order, OrderItem>("window", o => o.Items)
            .WithInception(DateTime.Now.AddDays(-1))
            .WithExpiration(DateTime.Now.AddDays(1))
            .Fire(item => { fired = true; });

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 1 }]
        };

        EvaluateSafe(ruleSet, order);

        fired.ShouldBeTrue();
    }


    // ========== FireOnce / FireAlways ==========

    [Fact]
    public void FireOnce_PreventsReEvaluation()
    {
        var ruleSet = new RuleSet();
        var fireCount = 0;

        ruleSet.AddRule<Order, OrderItem>("once", o => o.Items)
            .FireOnce()
            .Fire(item => fireCount++);

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 1 }]
        };

        EvaluateSafe(ruleSet, order);

        fireCount.ShouldBe(1);
    }

    [Fact]
    public void FireAlways_OverridesFireOnce()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Order, OrderItem>("always", o => o.Items)
            .FireOnce()
            .FireAlways()
            .Fire(item => { });

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Quantity = 1 }]
        };

        var result = EvaluateSafe(ruleSet, order);

        result.TotalFired.ShouldBeGreaterThanOrEqualTo(1);
    }


    // ========== And condition ==========

    [Fact]
    public void And_AddsAdditionalCondition()
    {
        var ruleSet = new RuleSet();
        var processed = new List<string>();

        // Test that And works by excluding items that fail the first condition
        ruleSet.AddRule<Order, OrderItem>("multi-cond", o => o.Items)
            .If(item => item.Quantity > 3)
            .And(item => item.Price > 0)
            .Then(item => processed.Add(item.Product));

        var order = new Order
        {
            OrderId = 1,
            Items =
            [
                new OrderItem { Product = "Widget", Quantity = 5, Price = 10 },
                new OrderItem { Product = "Cheap", Quantity = 1, Price = 5 },
                new OrderItem { Product = "Free", Quantity = 5, Price = 0 }
            ]
        };

        EvaluateSafe(ruleSet, order);

        // Widget: Qty>3 pass, Price>0 pass -> fires
        // Cheap: Qty>3 fail -> excluded by first condition
        // Free: Qty>3 pass, Price>0 fail -> excluded by second condition
        processed.ShouldContain("Widget");
        processed.ShouldNotContain("Cheap");
    }

}
