using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public interface IEmulationRegistry
{
    void Register<TService, TServiceConfig>(
        string key,
        EmulatorServiceFactory<TService, TServiceConfig> serviceFactory,
        IEmulationRegistryEntryApplier entryApplier);

    void Register<TService, TServiceConfig, THostedService, THostedServiceConfig>(
        string key,
        EmulatorServiceFactory<TService, TServiceConfig> serviceFactory,
        EmulatorHostedServiceFactory<THostedService, THostedServiceConfig> hostedServiceFactory,
        IEmulationRegistryEntryApplier entryApplier)
        where THostedService : class, IHostedService;

    bool TryGet(string key, out EmulationRegistryEntry entry);

    EmulationRegistryEntry Get(string key);

    IReadOnlyDictionary<string, EmulationRegistryEntry> GetAll();
}
