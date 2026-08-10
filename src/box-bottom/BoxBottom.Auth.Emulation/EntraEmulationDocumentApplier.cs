using System.Text.Json;
using BoxBottom.Auth.Emulation.Configuration;
using BoxBottom.Emulation;
using BoxBottom.Emulation.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Auth.Emulation;

public sealed class EntraEmulationDocumentApplier : IEmulationRegistryEntryApplier
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public void Apply(
        IHostApplicationBuilder builder,
        JsonDocument jsonDocument,
        IEmulationAnchorRepository anchorRepository,
        EmulationRegistryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(jsonDocument);
        ArgumentNullException.ThrowIfNull(anchorRepository);
        ArgumentNullException.ThrowIfNull(entry);

        var document = jsonDocument.Deserialize<EmulationConfigDocument<EntraEmulatorAnchorConfig, object?>>(
            SerializerOptions);

        if (document is null
            || !document.Singletons.TryGetValue(EntraEmulationRegistryExtensions.EntraAnchorName, out var config))
        {
            return;
        }

        var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Auth:Provider"] = config.Provider,
            ["Auth:Entra:TenantId"] = config.TenantId,
            ["Auth:Entra:Authority"] = config.Authority,
            ["Auth:Entra:ClientId"] = config.ClientId,
            ["Auth:Entra:ClientSecret"] = config.ClientSecret,
            ["Auth:Entra:PublicOrigin"] = config.PublicOrigin,
        };

        builder.Configuration.AddInMemoryCollection(
            overrides.Where(entry => !string.IsNullOrWhiteSpace(entry.Value)));
    }
}
