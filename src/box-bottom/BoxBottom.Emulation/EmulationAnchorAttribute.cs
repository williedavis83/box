namespace BoxBottom.Emulation;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EmulationAnchorAttribute : Attribute
{
    public EmulationAnchorAttribute(
        string anchorName,
        Type serviceType,
        EmulationAnchorKind kind,
        EmulationAnchorLifetime lifetime = EmulationAnchorLifetime.Singleton)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorName);
        ArgumentNullException.ThrowIfNull(serviceType);

        AnchorName = anchorName;
        ServiceType = serviceType;
        Kind = kind;
        Lifetime = lifetime;
    }

    public string AnchorName { get; }

    public Type ServiceType { get; }

    public EmulationAnchorKind Kind { get; }

    public EmulationAnchorLifetime Lifetime { get; }
}
