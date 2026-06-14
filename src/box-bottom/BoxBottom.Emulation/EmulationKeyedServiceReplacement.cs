using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Emulation;

public static class EmulationKeyedServiceReplacement
{
    public static void ReplaceKeyed(
        IServiceCollection services,
        Type serviceType,
        object? serviceKey,
        EmulationAnchorLifetime lifetime,
        Func<IServiceProvider, object?, object> implementationFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationFactory);

        RemoveKeyedService(services, serviceType, serviceKey);

        switch (lifetime)
        {
            case EmulationAnchorLifetime.Singleton:
                services.AddKeyedSingleton(serviceType, serviceKey, implementationFactory);
                break;
            case EmulationAnchorLifetime.Scoped:
                services.AddKeyedScoped(serviceType, serviceKey, implementationFactory);
                break;
            case EmulationAnchorLifetime.Transient:
                services.AddKeyedTransient(serviceType, serviceKey, implementationFactory);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Unsupported emulation anchor lifetime.");
        }
    }

    private static void RemoveKeyedService(
        IServiceCollection services,
        Type serviceType,
        object? serviceKey)
    {
        var descriptors = services
            .Where(descriptor =>
                descriptor.IsKeyedService &&
                descriptor.ServiceType == serviceType &&
                object.Equals(descriptor.ServiceKey, serviceKey))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
