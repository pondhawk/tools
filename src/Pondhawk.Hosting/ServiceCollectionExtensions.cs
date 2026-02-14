using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Pondhawk.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonWithStart<TService>(
        this IServiceCollection services,
        Action<TService> startAction)
        where TService : class
    {
        return services.AddSingletonWithStart<TService>(
            (svc, _) => { startAction(svc); return Task.CompletedTask; },
            null);
    }

    public static IServiceCollection AddSingletonWithStart<TService>(
        this IServiceCollection services,
        Action<TService> startAction,
        Action<TService> stopAction)
        where TService : class
    {
        return services.AddSingletonWithStart<TService>(
            (svc, _) => { startAction(svc); return Task.CompletedTask; },
            (svc, _) => { stopAction(svc); return Task.CompletedTask; });
    }

    public static IServiceCollection AddSingletonWithStart<TService>(
        this IServiceCollection services,
        Func<TService, CancellationToken, Task> startAction)
        where TService : class
    {
        return services.AddSingletonWithStart(startAction, null);
    }

    public static IServiceCollection AddSingletonWithStart<TService>(
        this IServiceCollection services,
        Func<TService, CancellationToken, Task> startAction,
        Func<TService, CancellationToken, Task> stopAction)
        where TService : class
    {
        services.AddSingleton<TService>();
        EnsureHostedService(services);

        services.AddSingleton(new ServiceStartDescriptor
        {
            ServiceType = typeof(TService),
            StartAction = (svc, ct) => startAction((TService)svc, ct),
            StopAction = stopAction is not null
                ? (svc, ct) => stopAction((TService)svc, ct)
                : (_, _) => Task.CompletedTask
        });

        return services;
    }

    private static void EnsureHostedService(IServiceCollection services)
    {
        services.TryAddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, ServiceStarterHostedService>());
    }
}
