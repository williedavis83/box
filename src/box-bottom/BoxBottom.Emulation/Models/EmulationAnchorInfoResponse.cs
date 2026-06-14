namespace BoxBottom.Emulation;

public sealed record EmulationAnchorInfoResponse(
    string AssemblyName,
    string AnchorName,
    string ServiceType,
    string Kind,
    string Lifetime,
    bool IsActivated)
{
    public static EmulationAnchorInfoResponse From(EmulationAnchorInfo anchor, IEmulationAnchorRepository repository)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(repository);

        return new EmulationAnchorInfoResponse(
            anchor.AssemblyName,
            anchor.AnchorName,
            anchor.ServiceType.FullName ?? anchor.ServiceType.Name,
            anchor.Kind.ToString(),
            anchor.Lifetime.ToString(),
            repository.IsActivated(anchor.AnchorName));
    }
}
