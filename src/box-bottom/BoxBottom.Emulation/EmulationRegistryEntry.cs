using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public sealed class EmulationRegistryEntry
{
    internal EmulationRegistryEntry(
        object serviceFactory,
        object? hostedServiceFactory = null)
    {
        ArgumentNullException.ThrowIfNull(serviceFactory);
        ServiceFactory = serviceFactory;
        HostedServiceFactory = hostedServiceFactory;
    }

    public object ServiceFactory { get; }

    public object? HostedServiceFactory { get; }

    public EmulatorServiceFactory<TService, TServiceConfig> GetServiceFactory<TService, TServiceConfig>() =>
        (EmulatorServiceFactory<TService, TServiceConfig>)ServiceFactory;

    public EmulatorHostedServiceFactory<THostedService, THostedServiceConfig>? GetHostedServiceFactory<
        THostedService,
        THostedServiceConfig>()
        where THostedService : class, IHostedService =>
        HostedServiceFactory as EmulatorHostedServiceFactory<THostedService, THostedServiceConfig>;
}
