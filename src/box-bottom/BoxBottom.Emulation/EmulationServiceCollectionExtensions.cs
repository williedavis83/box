using System.Reflection;
using System.Text.Json;
using BoxBottom.Emulation.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public static class EmulationServiceCollectionExtensions
{
    public static IHostApplicationBuilder EnableEmulationSupport(
        this IHostApplicationBuilder builder,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Environment.IsProduction())
        {
            return builder;
        }

        var repository = new EmulationAnchorRepository();

        foreach (var anchor in EmulationAnchorScanner.Scan(
                     EmulationAnchorScanner.CollectAssemblies(assemblies)))
        {
            repository.Add(anchor);
        }

        builder.Services.AddSingleton<IEmulationAnchorRepository>(repository);

        builder.Services.AddSingleton<IEmulationRegistry>(sp =>
        {
            var registry = new EmulationRegistry();

            foreach (var configurator in sp.GetServices<IEmulationRegistryConfigurator>())
            {
                configurator.Configure(registry);
            }

            return registry;
        });

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(EmulationDiagnosticController).Assembly);

        return builder;
    }

    public static IHostApplicationBuilder ApplyConfiguredEmulation(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Environment.IsProduction())
        {
            return builder;
        }

        var anchorRepository = GetSingletonInstance<IEmulationAnchorRepository>(builder.Services);
        if (anchorRepository is null)
        {
            return builder;
        }

        var appliers = GetSingletonInstances<IEmulationConfigurationApplier>(builder.Services).ToList();
        foreach (var applier in appliers)
        {
            var json = builder.Configuration[applier.EmulationKey];
            if (string.IsNullOrWhiteSpace(json))
            {
                continue;
            }

            using var jsonDocument = JsonDocument.Parse(json);
            applier.Apply(builder, jsonDocument, anchorRepository);
        }

        return builder;
    }

    private static T? GetSingletonInstance<T>(IServiceCollection services)
        where T : class
    {
        var descriptor = services.LastOrDefault(
            service => service.ServiceType == typeof(T) && service.ImplementationInstance is T);

        return descriptor?.ImplementationInstance as T;
    }

    private static IEnumerable<T> GetSingletonInstances<T>(IServiceCollection services)
        where T : class
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType != typeof(T))
            {
                continue;
            }

            if (descriptor.ImplementationInstance is T instance)
            {
                yield return instance;
            }
            else if (descriptor.ImplementationType is not null)
            {
                yield return (T)Activator.CreateInstance(descriptor.ImplementationType)!;
            }
        }
    }
}
