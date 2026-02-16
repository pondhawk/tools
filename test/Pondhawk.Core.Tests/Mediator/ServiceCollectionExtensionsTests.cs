using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Mediator;
using Shouldly;
using Xunit;

namespace Pondhawk.Core.Tests.Mediator;

public class ServiceCollectionExtensionsTests
{

    // ── Test doubles ──

    public record TestRequest(string Value) : IRequest<string>;

    public class TestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(request.Value);
        }
    }

    public record AnotherRequest : IRequest<int>;

    public class AnotherHandler : IRequestHandler<AnotherRequest, int>
    {
        public Task<int> HandleAsync(AnotherRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(99);
        }
    }

    public class TestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            return next();
        }
    }

    // ── AddMediator ──

    [Fact]
    public void AddMediator_RegistersMediator()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(TestHandler).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        mediator.ShouldBeOfType<Pondhawk.Mediator.Mediator>();
    }

    [Fact]
    public void AddMediator_DiscoversHandlersFromAssembly()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(TestHandler).Assembly);

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<IRequestHandler<TestRequest, string>>();
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<TestHandler>();
    }

    [Fact]
    public void AddMediator_DiscoversMultipleHandlers()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(TestHandler).Assembly);

        var provider = services.BuildServiceProvider();
        provider.GetService<IRequestHandler<TestRequest, string>>().ShouldNotBeNull();
        provider.GetService<IRequestHandler<AnotherRequest, int>>().ShouldNotBeNull();
    }

    [Fact]
    public async Task AddMediator_EndToEnd_HandlerResolvesAndExecutes()
    {
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestHandler).Assembly);
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new TestRequest("hello"));

        result.ShouldBe("hello");
    }

    [Fact]
    public void AddMediator_NullServices_Throws()
    {
        IServiceCollection services = null;

        Should.Throw<ArgumentNullException>(
            () => services.AddMediator(typeof(TestHandler).Assembly));
    }

    [Fact]
    public void AddMediator_NullAssemblies_Throws()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(
            () => services.AddMediator(null));
    }

    [Fact]
    public void AddMediator_EmptyAssemblies_Throws()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentException>(
            () => services.AddMediator(Array.Empty<Assembly>()));
    }

    // ── AddPipelineBehavior ──

    [Fact]
    public void AddPipelineBehavior_RegistersOpenGenericBehavior()
    {
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestHandler).Assembly);

        services.AddPipelineBehavior(typeof(TestBehavior<,>));

        var provider = services.BuildServiceProvider();
        var behaviors = provider.GetServices<IPipelineBehavior<TestRequest, string>>();
        behaviors.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddPipelineBehavior_NullServices_Throws()
    {
        IServiceCollection services = null;

        Should.Throw<ArgumentNullException>(
            () => services.AddPipelineBehavior(typeof(TestBehavior<,>)));
    }

    [Fact]
    public void AddPipelineBehavior_NullBehaviorType_Throws()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(
            () => services.AddPipelineBehavior(null));
    }

}
