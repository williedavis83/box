using Blabber.Emulator.Configuration;
using Blabber.Emulator.Options;
using BoxBottom.Emulation.Shared;

namespace Blabber.Emulator;

public sealed class BlabberEmulationConfigurationBuilder
    : IEmulationConfigurationBuilder<BlabberEmulatorAnchorConfig, BlabberEmulatorHostedServiceConfig>
{
    private readonly Dictionary<string, BlabberEmulatorAnchorConfig> _singletons =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, List<BlabberEmulatorAnchorConfig>> _lists =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, Dictionary<string, BlabberEmulatorAnchorConfig>> _dictionaries =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly List<BlabberEmulatorAccount> _seedAccounts = [];

    public string EmulationKey => BlabberEmulationRegistryExtensions.BlabberEmulationKey;

    public BlabberEmulationConfigurationBuilder OverrideSingleton(string anchorName, string account)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorName);
        ArgumentException.ThrowIfNullOrWhiteSpace(account);

        _singletons[anchorName] = new BlabberEmulatorAnchorConfig(account);
        return this;
    }

    public BlabberEmulationConfigurationBuilder OverrideList(string anchorName, params string[] accounts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorName);
        ArgumentNullException.ThrowIfNull(accounts);

        _lists[anchorName] = accounts
            .Select(account => new BlabberEmulatorAnchorConfig(account))
            .ToList();
        return this;
    }

    public BlabberEmulationConfigurationBuilder OverrideDictionary(
        string anchorName,
        params (string Key, string Account)[] entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorName);
        ArgumentNullException.ThrowIfNull(entries);

        _dictionaries[anchorName] = entries.ToDictionary(
            entry => entry.Key,
            entry => new BlabberEmulatorAnchorConfig(entry.Account),
            StringComparer.OrdinalIgnoreCase);
        return this;
    }

    public BlabberEmulationConfigurationBuilder WithSeedAccounts(params BlabberEmulatorAccount[] accounts)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        _seedAccounts.AddRange(accounts);
        return this;
    }

    public void OrchestrateEmulationResources(IEmulationResourceOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);

        if (!RequiresEmulatorResource())
        {
            return;
        }

        if (orchestrator.TryGetOrchestratedResource(
                BleebEmulatorOrchestrationConfiguration.ResourceName,
                out _))
        {
            return;
        }

        orchestrator.OrchestrateSupportProject(
            BleebEmulatorOrchestrationConfiguration.ResourceName,
            BleebEmulatorOrchestrationConfiguration.ApiProjectPath);
    }

    public EmulationConfigDocument<BlabberEmulatorAnchorConfig, BlabberEmulatorHostedServiceConfig> BuildDocument()
    {
        return new EmulationConfigDocument<BlabberEmulatorAnchorConfig, BlabberEmulatorHostedServiceConfig>
        {
            Singletons = new Dictionary<string, BlabberEmulatorAnchorConfig>(_singletons, StringComparer.OrdinalIgnoreCase),
            Lists = new Dictionary<string, List<BlabberEmulatorAnchorConfig>>(
                _lists.ToDictionary(
                    entry => entry.Key,
                    entry => new List<BlabberEmulatorAnchorConfig>(entry.Value),
                    StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase),
            Dictionaries = new Dictionary<string, Dictionary<string, BlabberEmulatorAnchorConfig>>(
                _dictionaries.ToDictionary(
                    entry => entry.Key,
                    entry => new Dictionary<string, BlabberEmulatorAnchorConfig>(entry.Value, StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase),
            HostedServiceConfig = _seedAccounts.Count == 0
                ? null
                : new BlabberEmulatorHostedServiceConfig
                {
                    Accounts = [.._seedAccounts],
                },
        };
    }

    private bool RequiresEmulatorResource() =>
        _singletons.Count > 0 || _lists.Count > 0 || _dictionaries.Count > 0 || _seedAccounts.Count > 0;
}
