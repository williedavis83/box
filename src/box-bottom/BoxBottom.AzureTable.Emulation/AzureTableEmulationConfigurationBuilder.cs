using BoxBottom.AzureTable.Emulation.Configuration;
using BoxBottom.Emulation.Shared;

namespace BoxBottom.AzureTable.Emulation;

public sealed class AzureTableEmulationConfigurationBuilder
    : IEmulationConfigurationBuilder<AzureTableEmulatorAnchorConfig, object?>
{
    private readonly Dictionary<string, AzureTableEmulatorAnchorConfig> _singletons =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, Dictionary<string, AzureTableEmulatorAnchorConfig>> _dictionaries =
        new(StringComparer.OrdinalIgnoreCase);

    public string EmulationKey => AzureTableEmulationRegistryExtensions.AzureTableEmulationKey;

    public AzureTableEmulationConfigurationBuilder OverrideSingleton(string anchorName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorName);
        _singletons[anchorName] = new AzureTableEmulatorAnchorConfig();
        return this;
    }

    public AzureTableEmulationConfigurationBuilder OverrideDictionary(
        string anchorName,
        params (string DictionaryKey, string MemberAnchorKey)[] entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorName);
        ArgumentNullException.ThrowIfNull(entries);

        _dictionaries[anchorName] = entries.ToDictionary(
            entry => entry.DictionaryKey,
            entry => new AzureTableEmulatorAnchorConfig(entry.MemberAnchorKey),
            StringComparer.OrdinalIgnoreCase);

        return this;
    }

    public void OrchestrateEmulationResources(IEmulationResourceOrchestrator orchestrator)
    {
        // Azurite orchestration is handled by BoxBottom.AzureTable.Aspire, not the general emulation library.
    }

    public EmulationConfigDocument<AzureTableEmulatorAnchorConfig, object?> BuildDocument() =>
        new()
        {
            Singletons = new Dictionary<string, AzureTableEmulatorAnchorConfig>(_singletons, StringComparer.OrdinalIgnoreCase),
            Dictionaries = new Dictionary<string, Dictionary<string, AzureTableEmulatorAnchorConfig>>(
                _dictionaries.ToDictionary(
                    entry => entry.Key,
                    entry => new Dictionary<string, AzureTableEmulatorAnchorConfig>(entry.Value, StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase),
        };
}
