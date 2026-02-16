using Microsoft.Extensions.DependencyInjection;
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

        result.ShouldBe("Pong: hello");
    }

    [Fact]
    public async Task SendAsync_CommandAlias_RoutesToHandler()
    {
        var mediator = BuildMediator(s =>
            s.AddScoped<IRequestHandler<CreateOrder, int>, CreateOrderHandler>());

        var result = await mediator.SendAsync(new CreateOrder("Test"));

        result.ShouldBe(42);
    }

    [Fact]
    public async Task SendAsync_QueryAlias_RoutesToHandler()
    {
        var mediator = BuildMediator(s =>
            s.AddScoped<IRequestHandler<GetOrder, string>, GetOrderHandler>());

        var result = await mediator.SendAsync(new GetOrder(7));

        result.ShouldBe("Order-7");
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

        r1.ShouldBe("Pong: first");
        r2.ShouldBe("Pong: second");
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

        result.ShouldBe("Pong: test");
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

        result.ShouldBeNull();
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

}
