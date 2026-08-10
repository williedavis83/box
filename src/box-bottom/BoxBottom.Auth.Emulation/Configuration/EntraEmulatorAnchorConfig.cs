namespace BoxBottom.Auth.Emulation.Configuration;

public sealed class EntraEmulatorAnchorConfig
{
    public string Provider { get; init; } = "Entra";

    public string? TenantId { get; init; }

    public string? Authority { get; init; }

    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public string? PublicOrigin { get; init; }
}
