namespace BoxBottom.Emulation;

public sealed record EmulationAnchorInfo(
    string AssemblyName,
    string AnchorName,
    Type ServiceType,
    EmulationAnchorKind Kind,
    EmulationAnchorLifetime Lifetime);
