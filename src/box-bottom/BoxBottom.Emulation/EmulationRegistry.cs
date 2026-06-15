using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public sealed class EmulationRegistry : IEmulationRegistry
{
    private readonly Dictionary<string, EmulationRegistryEntry> _entries =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register<TService, TServiceConfig>(
        string key,
        EmulatorServiceFactory<TService, TServiceConfig> serviceFactory,
        IEmulationRegistryEntryApplier entryApplier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(serviceFactory);
        ArgumentNullException.ThrowIfNull(entryApplier);

        _entries[key] = new EmulationRegistryEntry(serviceFactory, entryApplier);
    }

    public void Register<TService, TServiceConfig, THostedService, THostedServiceConfig>(
        string key,
        EmulatorServiceFactory<TService, TServiceConfig> serviceFactory,
        EmulatorHostedServiceFactory<THostedService, THostedServiceConfig> hostedServiceFactory,
        IEmulationRegistryEntryApplier entryApplier)
        where THostedService : class, IHostedService
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(serviceFactory);
        ArgumentNullException.ThrowIfNull(hostedServiceFactory);
        ArgumentNullException.ThrowIfNull(entryApplier);

        _entries[key] = new EmulationRegistryEntry(serviceFactory, entryApplier, hostedServiceFactory);
    }

    public bool TryGet(string key, out EmulationRegistryEntry entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_entries.TryGetValue(key, out var existing))
        {
            entry = existing;
            return true;
        }

        entry = null!;
        return false;
    }

    public EmulationRegistryEntry Get(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!_entries.TryGetValue(key, out var entry))
        {
            throw new KeyNotFoundException($"No emulation registry entry exists for key '{key}'.");
        }

        return entry;
    }

    public IReadOnlyDictionary<string, EmulationRegistryEntry> GetAll() =>
        new Dictionary<string, EmulationRegistryEntry>(_entries, StringComparer.OrdinalIgnoreCase);
}
