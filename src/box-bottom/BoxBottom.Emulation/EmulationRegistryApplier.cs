using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public static class EmulationRegistryApplier
{
    public static void Apply(
        IHostApplicationBuilder builder,
        IEmulationRegistry registry,
        IEmulationAnchorRepository anchorRepository)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(anchorRepository);

        foreach (var (key, entry) in registry.GetAll())
        {
            var json = builder.Configuration[key];
            if (string.IsNullOrWhiteSpace(json))
            {
                continue;
            }

            if (entry.EntryApplier is null)
            {
                throw new InvalidOperationException(
                    $"Emulation registry entry '{key}' has configuration but no entry applier was registered.");
            }

            using var jsonDocument = JsonDocument.Parse(json);
            entry.EntryApplier.Apply(builder, jsonDocument, anchorRepository, entry);
        }
    }
}
