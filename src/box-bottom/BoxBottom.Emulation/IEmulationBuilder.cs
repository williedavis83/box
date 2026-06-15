using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public interface IEmulationBuilder
{
    IHostApplicationBuilder HostBuilder { get; }

    IEmulationAnchorRepository AnchorRepository { get; }

    IEmulationRegistry Registry { get; }

    IEmulationBuilder AddRegistryConfigurator(IEmulationRegistryConfigurator configurator);

    IEmulationBuilder ApplyConfiguredEmulation();
}
