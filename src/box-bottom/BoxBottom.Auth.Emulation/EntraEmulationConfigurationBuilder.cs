using BoxBottom.Auth.Emulation.Configuration;
using BoxBottom.Emulation.Shared;

namespace BoxBottom.Auth.Emulation;

public sealed class EntraEmulationConfigurationBuilder
    : IEmulationConfigurationBuilder<EntraEmulatorAnchorConfig, object?>
{
    private readonly Dictionary<string, EntraEmulatorAnchorConfig> _singletons =
        new(StringComparer.OrdinalIgnoreCase);

    public string EmulationKey => EntraEmulationRegistryExtensions.EntraEmulationKey;

    public EntraEmulationConfigurationBuilder OverrideEntra(
        EntraEmulatorAnchorConfig? config = null)
    {
        _singletons["Entra"] = config ?? new EntraEmulatorAnchorConfig();
        return this;
    }

    public void OrchestrateEmulationResources(IEmulationResourceOrchestrator orchestrator)
    {
        // Keycloak container orchestration is handled by BoxBottom.Auth.Aspire.
    }

    public EmulationConfigDocument<EntraEmulatorAnchorConfig, object?> BuildDocument() =>
        new()
        {
            Singletons = new Dictionary<string, EntraEmulatorAnchorConfig>(_singletons, StringComparer.OrdinalIgnoreCase),
        };
}
