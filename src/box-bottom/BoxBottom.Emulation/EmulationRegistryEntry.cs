using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public sealed class EmulationRegistryEntry
{
    internal EmulationRegistryEntry(
        object serviceFactory,
        IEmulationRegistryEntryApplier entryApplier,
        object? hostedServiceFactory = null)
    {
        ArgumentNullException.ThrowIfNull(serviceFactory);
        ArgumentNullException.ThrowIfNull(entryApplier);

        ServiceFactory = serviceFactory;
        EntryApplier = entryApplier;
        HostedServiceFactory = hostedServiceFactory;
    }

    public object ServiceFactory { get; }

    public object? HostedServiceFactory { get; }

    public IEmulationRegistryEntryApplier EntryApplier { get; }

    public EmulatorServiceFactory<TService, TServiceConfig> GetServiceFactory<TService, TServiceConfig>() =>
        (EmulatorServiceFactory<TService, TServiceConfig>)ServiceFactory;

    public EmulatorHostedServiceFactory<THostedService, THostedServiceConfig>? GetHostedServiceFactory<
        THostedService,
        THostedServiceConfig>()
        where THostedService : class, IHostedService =>
        HostedServiceFactory as EmulatorHostedServiceFactory<THostedService, THostedServiceConfig>;
}
