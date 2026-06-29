using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pondhawk.Exceptions;
using Pondhawk.Mediator;
using Shouldly;
using Xunit;

namespace Pondhawk.Core.Tests.Mediator;

public class MediatorTests
{

    // ── Test doubles ──

    public record Ping(string Message) : IRequest<string>;

    public class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> HandleAsync(Ping request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Pong: {request.Message}");
        }
    }

    public record CreateOrder(string Name) : ICommand<int>;

    public class CreateOrderHandler : ICommandHandler<CreateOrder, int>
    {
        public Task<int> HandleAsync(CreateOrder request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(42);
        }
    }

    public record GetOrder(int Id) : IQuery<string>;

    public class GetOrderHandler : IQueryHandler<GetOrder, string>
    {
        public Task<string> HandleAsync(GetOrder request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Order-{request.Id}");
        }
    }

    public record NoHandlerRequest : IRequest<string>;

    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public List<string> Log { get; } = [];

        public async Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            Log.Add($"Before:{typeof(TRequest).Name}");
            var response = await next();
            Log.Add($"After:{typeof(TRequest).Name}");
            return response;
        }
    }

    public class ShortCircuitBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            return default;
        }
    }

    // ── Throwing handlers (for the enveloping seam) ──

    public record FindThing(int Id) : IRequest<string>;

    public class FindThingHandler : IRequestHandler<FindThing, string>
    {
        public Task<string> HandleAsync(FindThing request, CancellationToken cancellationToken = default)
            => throw new NotFoundException("Thing", request.Id);
    }

    public record ValidateThing : IRequest<string>;

    public class ValidateThingHandler : IRequestHandler<ValidateThing, string>
    {
        public Task<string> HandleAsync(ValidateThing request, CancellationToken cancellationToken = default)
            => throw new FailedValidationException(
            [
                EventDetail.Build()
                    .WithRuleName("R1")
                    .WithCategory(EventDetail.EventCategory.Violation)
                    .WithExplanation("Name is required"),
            ]);
    }

    public record BoomThing : IRequest<string>;

    public class BoomThingHandler : IRequestHandler<BoomThing, string>
    {
        public Task<string> HandleAsync(BoomThing request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("kaboom");
    }

    public record CancelThing : IRequest<string>;

    public class CancelThingHandler : IRequestHandler<CancelThing, string>
    {
        public Task<string> HandleAsync(CancelThing request, CancellationToken cancellationToken = default)
            => throw new OperationCanceledException();
    }

    // ── Helpers ──

    private static IMediator BuildMediator(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        services.AddScoped<IMediator, Pondhawk.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMediator>();
    }

    // ── SendAsync ──

    [Fact]
    public async Task SendAsync_RoutesToHandler_ReturnsResponse()
    {
        var mediator = BuildMediator(s =>
            s.AddScoped<IRequestHandler<Ping, string>, PingHandler>());

        var result = await mediator.SendAsync(new Ping("hello"));

        result.Ok.ShouldBeTrue();
        result.Value.ShouldBe("Pong: hello");
    }

    [Fact]
    public async Task SendAsync_CommandAlias_RoutesToHandler()
    {
        var mediator = BuildMediator(s =>
            s.AddScoped<IRequestHandler<CreateOrder, int>, CreateOrderHandler>());

        var result = await mediator.SendAsync(new CreateOrder("Test"));

        result.Ok.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public async Task SendAsync_QueryAlias_RoutesToHandler()
    {
        var mediator = BuildMediator(s =>
            s.AddScoped<IRequestHandler<GetOrder, string>, GetOrderHandler>());

        var result = await mediator.SendAsync(new GetOrder(7));

        result.Ok.ShouldBeTrue();
        result.Value.ShouldBe("Order-7");
    }

    [Fact]
    public async Task SendAsync_NullRequest_Throws()
    {
        var mediator = BuildMediator(_ => { });

        await Should.ThrowAsync<ArgumentNullException>(
            () => mediator.SendAsync<string>(null));
    }

    [Fact]
    public async Task SendAsync_NoHandler_ThrowsInvalidOperation()
    {
        var mediator = BuildMediator(_ => { });

        await Should.ThrowAsync<InvalidOperationException>(
            () => mediator.SendAsync(new NoHandlerRequest()));
    }

    [Fact]
    public async Task SendAsync_CachesHandlerWrapper_SecondCallSucceeds()
    {
        var mediator = BuildMediator(s =>
            s.AddScoped<IRequestHandler<Ping, string>, PingHandler>());

        var r1 = await mediator.SendAsync(new Ping("first"));
        var r2 = await mediator.SendAsync(new Ping("second"));

        r1.Value.ShouldBe("Pong: first");
        r2.Value.ShouldBe("Pong: second");
    }

    // ── Pipeline behaviors ──

    [Fact]
    public async Task SendAsync_WithBehavior_ExecutesAroundHandler()
    {
        var behavior = new LoggingBehavior<Ping, string>();

        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<Ping, string>, PingHandler>();
        services.AddSingleton<IPipelineBehavior<Ping, string>>(behavior);
        services.AddScoped<IMediator, Pondhawk.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new Ping("test"));

        result.Value.ShouldBe("Pong: test");
        behavior.Log.Count.ShouldBe(2);
        behavior.Log[0].ShouldBe("Before:Ping");
        behavior.Log[1].ShouldBe("After:Ping");
    }

    [Fact]
    public async Task SendAsync_MultipleBehaviors_ExecuteInRegistrationOrder()
    {
        var callOrder = new List<string>();

        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<Ping, string>, PingHandler>();
        services.AddSingleton<IPipelineBehavior<Ping, string>>(new OrderTrackingBehavior<Ping, string>("Outer", callOrder));
        services.AddSingleton<IPipelineBehavior<Ping, string>>(new OrderTrackingBehavior<Ping, string>("Inner", callOrder));
        services.AddScoped<IMediator, Pondhawk.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new Ping("test"));

        callOrder.ShouldBe(["Outer:Before", "Inner:Before", "Inner:After", "Outer:After"]);
    }

    [Fact]
    public async Task SendAsync_BehaviorCanShortCircuit()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<Ping, string>, PingHandler>();
        services.AddSingleton<IPipelineBehavior<Ping, string>>(new ShortCircuitBehavior<Ping, string>());
        services.AddScoped<IMediator, Pondhawk.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new Ping("test"));

        result.Ok.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    private class OrderTrackingBehavior<TRequest, TResponse>(string name, List<string> callOrder)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            callOrder.Add($"{name}:Before");
            var response = await next();
            callOrder.Add($"{name}:After");
            return response;
        }
    }

    // ── Enveloping seam ──

    [Fact]
    public async Task SendAsync_ExternalException_EnvelopesWithKind()
    {
        var mediator = BuildMediator(s =>
            s.AddScoped<IRequestHandler<FindThing, string>, FindThingHandler>());

        var result = await mediator.SendAsync(new FindThing(7));

        result.Ok.ShouldBeFalse();
        result.Value.ShouldBeNull();
        result.Error.ShouldNotBeNull();
        result.Error!.Kind.ShouldBe(ErrorKind.NotFound);
        result.Error.ErrorCode.ShouldBe("NotFound");
        result.Error.Explanation.ShouldContain("7");
    }

    [Fact]
    public async Task SendAsync_ValidationFailure_CarriesViolationsInDetails()
    {
        var mediator = BuildMediator(s =>
            s.AddScoped<IRequestHandler<ValidateThing, string>, ValidateThingHandler>());

        var result = await mediator.SendAsync(new ValidateThing());

        result.Ok.ShouldBeFalse();
        result.Error!.Kind.ShouldBe(ErrorKind.Predicate);
        result.Error.Details.ShouldNotBeEmpty();
        result.Error.Details[0].Explanation.ShouldBe("Name is required");
    }

    [Fact]
    public async Task SendAsync_UnexpectedException_EnvelopesAsSystem_AndLogsError()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<BoomThing, string>, BoomThingHandler>();
        var provider = services.BuildServiceProvider();

        var logger = new ListLogger<Pondhawk.Mediator.Mediator>();
        var mediator = new Pondhawk.Mediator.Mediator(provider, logger);

        var result = await mediator.SendAsync(new BoomThing());

        result.Ok.ShouldBeFalse();
        result.Error!.Kind.ShouldBe(ErrorKind.System);
        result.Error.Explanation.ShouldBe("kaboom");
        logger.Entries.ShouldContain(e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task SendAsync_ExpectedException_NotLoggedAsError()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<FindThing, string>, FindThingHandler>();
        var provider = services.BuildServiceProvider();

        var logger = new ListLogger<Pondhawk.Mediator.Mediator>();
        var mediator = new Pondhawk.Mediator.Mediator(provider, logger);

        await mediator.SendAsync(new FindThing(1));

        logger.Entries.ShouldNotContain(e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task SendAsync_OperationCanceled_Propagates()
    {
        var mediator = BuildMediator(s =>
            s.AddScoped<IRequestHandler<CancelThing, string>, CancelThingHandler>());

        await Should.ThrowAsync<OperationCanceledException>(
            () => mediator.SendAsync(new CancelThing()));
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }

}
