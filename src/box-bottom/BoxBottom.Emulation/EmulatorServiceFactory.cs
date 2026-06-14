using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Emulation;

public sealed class EmulatorServiceFactory<TServiceType, TServiceTypeConfig>
    : IEmulatorServiceFactory<TServiceTypeConfig>
{
    private readonly Func<TServiceTypeConfig, IServiceProvider, TServiceType> _createService;

    public EmulatorServiceFactory(Func<TServiceTypeConfig, IServiceProvider, TServiceType> createService)
    {
        ArgumentNullException.ThrowIfNull(createService);
        _createService = createService;
    }

    public TServiceType CreateSingleton(TServiceTypeConfig config, IServiceProvider serviceProvider) =>
        _createService(config, serviceProvider);

    public IReadOnlyList<TServiceType> CreateList(
        IReadOnlyList<TServiceTypeConfig> configs,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(configs);

        return configs
            .Select(config => _createService(config, serviceProvider))
            .ToList();
    }

    public IReadOnlyDictionary<string, TServiceType> CreateDictionary(
        IReadOnlyDictionary<string, TServiceTypeConfig> configs,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(configs);

        return configs.ToDictionary(
            entry => entry.Key,
            entry => _createService(entry.Value, serviceProvider));
    }

    object IEmulatorServiceFactory<TServiceTypeConfig>.CreateSingleton(
        TServiceTypeConfig config,
        IServiceProvider serviceProvider) =>
        CreateSingleton(config, serviceProvider)!;

    object IEmulatorServiceFactory<TServiceTypeConfig>.CreateList(
        IReadOnlyList<TServiceTypeConfig> configs,
        IServiceProvider serviceProvider) =>
        CreateList(configs, serviceProvider);

    object IEmulatorServiceFactory<TServiceTypeConfig>.CreateDictionary(
        IReadOnlyDictionary<string, TServiceTypeConfig> configs,
        IServiceProvider serviceProvider) =>
        CreateDictionary(configs, serviceProvider);
}
