namespace BoxBottom.Aspire.Orchestration;

/// <summary>
/// AppHost configuration for Dapr sidecar attachment.
/// Default is off; API-to-API references turn sidecars on for both ends.
/// </summary>
public static class DaprSidecarConfiguration
{
    public const string SectionName = "Dapr:Sidecar";
    public const string StartKey = "Dapr:Sidecar:Start";
}
