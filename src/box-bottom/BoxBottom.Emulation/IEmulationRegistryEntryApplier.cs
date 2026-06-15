using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public interface IEmulationRegistryEntryApplier
{
    void Apply(
        IHostApplicationBuilder builder,
        JsonDocument jsonDocument,
        IEmulationAnchorRepository anchorRepository,
        EmulationRegistryEntry entry);
}
