using Pondhawk.Rules.Factory;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class ForeachRuleTests
{

    [Fact]
    public void ForeachRule_IteratesOverChildren()
    {
        var ruleSet = new RuleSet();
        var processed = new List<string>();

        ruleSet.AddRule<Order, OrderItem>("item-rule", o => o.Items)
            .If(item => item.Quantity > 0)
            .Then(item => processed.Add(item.Product));

        var order = new Order
        {
            OrderId = 1,
            CustomerName = "Alice",
            Items =
            [
                new OrderItem { Product = "Widget", Quantity = 5 },
                new OrderItem { Product = "Gadget", Quantity = 0 },
                new OrderItem { Product = "Doohickey", Quantity = 3 }
            ]
        };

        ruleSet.Evaluate(order);

        processed.Count.ShouldBe(2);
        processed.ShouldContain("Widget");
        processed.ShouldContain("Doohickey");
        processed.ShouldNotContain("Gadget");
    }

    [Fact]
    public void ForeachRule_EmptyCollection_NothingFires()
    {
        var ruleSet = new RuleSet();
        var fired = false;

        ruleSet.AddRule<Order, OrderItem>("item-rule", o => o.Items)
            .Fire(item => { fired = true; });

        var order = new Order { OrderId = 1, Items = [] };
        ruleSet.Evaluate(order);

        fired.ShouldBeFalse();
    }

    [Fact]
    public void ForeachRule_WithModify_TriggersForwardChaining()
    {
        var ruleSet = new RuleSet();
        var fireCount = 0;

        ruleSet.AddRule<Order, OrderItem>("item-modifier", o => o.Items)
            .WithSalience(100)
            .If(item => item.Price == 0)
            .Then(item => { item.Price = 9.99m; fireCount++; })
            .Modifies();

        ruleSet.AddRule<Order>("order-check")
            .WithSalience(200)
            .If(o => o.Items.All(i => i.Price > 0))
            .Then(o => { fireCount++; });

        var order = new Order
        {
            OrderId = 1,
            Items = [new OrderItem { Product = "Widget", Price = 0, Quantity = 1 }]
        };

        ruleSet.Evaluate(order);

        fireCount.ShouldBeGreaterThanOrEqualTo(2);
    }

}
