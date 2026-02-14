using Pondhawk.Exceptions;
using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Factory;
using Pondhawk.Rules.Listeners;
using Shouldly;
using Serilog;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class ListenerTests
{

    // ========== NoopEvaluationListener ==========

    [Fact]
    public void NoopListener_AllMethodsExecuteWithoutError()
    {
        var listener = new NoopEvaluationListener();

        listener.BeginEvaluation();
        listener.BeginTupleEvaluation([new object()]);
        listener.FiringRule(new Rule<Person>("test", "rule"));
        listener.FiredRule(new Rule<Person>("test", "rule"), false);
        listener.FiredRule(new Rule<Person>("test", "rule"), true);
        listener.EndTupleEvaluation([new object()]);
        listener.EndEvaluation();
        listener.Debug("test {0}", "value");
        listener.Warning("warn {0}", "value");
        listener.EventCreated(new EventDetail
        {
            Category = EventDetail.EventCategory.Info,
            Explanation = "test",
            Group = "",
            RuleName = "",
            Source = ""
        });
    }


    // ========== WatchEvaluationListener construction ==========

    [Fact]
    public void WatchListener_CanBeConstructedWithLogger()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var listener = new WatchEvaluationListener(logger);

        listener.ShouldNotBeNull();
    }

    [Fact]
    public void WatchListener_CanBeConstructedWithCategory()
    {
        var listener = new WatchEvaluationListener("TestCategory");

        listener.ShouldNotBeNull();
    }


    // ========== WatchEvaluationListenerFactory ==========

    [Fact]
    public void WatchListenerFactory_CreatesListener()
    {
        var factory = new WatchEvaluationListenerFactory
        {
            Category = "TestCategory"
        };

        var listener = factory.CreateListener();

        listener.ShouldNotBeNull();
        listener.ShouldBeOfType<WatchEvaluationListener>();
    }

    [Fact]
    public void WatchListenerFactory_CategoryProperty()
    {
        var factory = new WatchEvaluationListenerFactory();

        factory.Category.ShouldBe(string.Empty);

        factory.Category = "MyCategory";
        factory.Category.ShouldBe("MyCategory");
    }


    // ========== Listener integration with evaluation ==========

    [Fact]
    public void Listener_IntegratesWithEvaluationContext()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("test")
            .Fire(p => { });

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowNoRulesException = false;
        ctx.ThrowValidationException = false;
        ctx.Listener = new NoopEvaluationListener();
        ctx.AddFacts(new Person { Name = "Test" });

        var result = ruleSet.Evaluate(ctx);

        result.TotalFired.ShouldBe(1);
    }

    [Fact]
    public void Listener_WatchListener_IntegratesWithEvaluation()
    {
        var ruleSet = new RuleSet();

        ruleSet.AddRule<Person>("test")
            .Fire(p => { });

        var logger = new LoggerConfiguration().CreateLogger();

        var ctx = ruleSet.GetEvaluationContext();
        ctx.ThrowNoRulesException = false;
        ctx.ThrowValidationException = false;
        ctx.Listener = new WatchEvaluationListener(logger);
        ctx.AddFacts(new Person { Name = "Test" });

        var result = ruleSet.Evaluate(ctx);

        result.TotalFired.ShouldBe(1);
    }

    [Fact]
    public void Listener_DefaultIsNoop()
    {
        var ctx = new EvaluationContext();

        ctx.Listener.ShouldBeOfType<NoopEvaluationListener>();
    }

}
