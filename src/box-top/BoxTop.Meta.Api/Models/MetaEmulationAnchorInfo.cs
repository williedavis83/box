namespace BoxTop.Meta.Api.Models;

public sealed record MetaEmulationAnchorInfo(
    string AssemblyName,
    string AnchorName,
    string ServiceType,
    string Kind,
    string Lifetime,
    bool IsActivated);
