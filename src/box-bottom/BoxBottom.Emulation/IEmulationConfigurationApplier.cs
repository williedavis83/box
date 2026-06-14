using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public interface IEmulationConfigurationApplier
{
    string EmulationKey { get; }

    void Apply(
        IHostApplicationBuilder builder,
        JsonDocument jsonDocument,
        IEmulationAnchorRepository anchorRepository);
}
