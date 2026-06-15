using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

internal sealed class EmulationBuilder : IEmulationBuilder
{
    private readonly List<IEmulationRegistryConfigurator> _configurators = [];

    public EmulationBuilder(
        IHostApplicationBuilder hostBuilder,
        IEmulationAnchorRepository anchorRepository,
        IEmulationRegistry registry)
    {
        HostBuilder = hostBuilder;
        AnchorRepository = anchorRepository;
        Registry = registry;
    }

    public IHostApplicationBuilder HostBuilder { get; }

    public IEmulationAnchorRepository AnchorRepository { get; }

    public IEmulationRegistry Registry { get; }

    public void AddConfigurator(IEmulationRegistryConfigurator configurator)
    {
        ArgumentNullException.ThrowIfNull(configurator);
        _configurators.Add(configurator);
    }

    public IEmulationBuilder AddRegistryConfigurator(IEmulationRegistryConfigurator configurator)
    {
        AddConfigurator(configurator);
        return this;
    }

    public IEmulationBuilder ApplyConfiguredEmulation()
    {
        foreach (var configurator in _configurators)
        {
            configurator.Configure(Registry);
        }

        EmulationRegistryApplier.Apply(HostBuilder, Registry, AnchorRepository);
        return this;
    }
}
