namespace BoxBottom.Emulation.Shared;

public sealed class EmulationConfigDocument<TEmulatorConfig, THostedServiceConfig>
{
    public Dictionary<string, TEmulatorConfig> Singletons { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, List<TEmulatorConfig>> Lists { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, Dictionary<string, TEmulatorConfig>> Dictionaries { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public THostedServiceConfig? HostedServiceConfig { get; init; }
}
