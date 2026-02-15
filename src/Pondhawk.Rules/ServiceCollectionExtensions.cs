using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Factory;

namespace Pondhawk.Rules;

/// <summary>
/// Extension methods for registering the Pondhawk Rules engine with Microsoft DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a <see cref="Factory.RuleSetFactory"/> singleton and a transient <see cref="IRuleSet"/>.
    /// Call <see cref="AddRules(IServiceCollection, IRuleBuilderSource)"/> first to register rule builder sources.
    /// </summary>
    public static IServiceCollection UseRules(this IServiceCollection services)
    {


        // *********************************************************
        services.AddSingleton(sp =>
            {

                var sources = sp.GetServices<IRuleBuilderSource>();

                var comp = new RuleSetFactory();
                comp.AddAllSources(sources);

                return comp;

            });



        // *********************************************************
        services.AddTransient<IRuleSet>(sp => sp.GetRequiredService<RuleSetFactory>().GetRuleSet());



        // *********************************************************
        return services;

    }



    public static IServiceCollection AddRules(this IServiceCollection services, IRuleBuilderSource source)
    {


        // *********************************************************
        services.AddSingleton<IRuleBuilderSource>(source);



        // *********************************************************
        return services;


    }


    public static IServiceCollection AddRules(this IServiceCollection services, params Assembly[] assemblies)
    {


        // *********************************************************
        services.AddSingleton<IRuleBuilderSource>(sp =>
            {

                var comp = new RuleBuilderSource();
                comp.AddTypes(assemblies);

                return comp;

            });



        // *********************************************************
        return services;


    }


    public static IServiceCollection AddRules(this IServiceCollection services, params Type[] types)
    {


        // *********************************************************
        services.AddSingleton<IRuleBuilderSource>(sp =>
        {

            var comp = new RuleBuilderSource();
            comp.AddTypes(types);

            return comp;

        });


        // *********************************************************
        return services;


    }


    public static IServiceCollection AddRules(this IServiceCollection services, IEnumerable<Type> candidates)
    {


        // *********************************************************
        services.AddSingleton<IRuleBuilderSource>(sp =>
        {

            var comp = new RuleBuilderSource();
            comp.AddTypes(candidates);

            return comp;

        });


        // *********************************************************
        return services;


    }


}


