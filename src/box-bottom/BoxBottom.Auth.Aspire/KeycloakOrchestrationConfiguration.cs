namespace BoxBottom.Auth.Aspire;

public static class KeycloakOrchestrationConfiguration
{
    public const string ResourceName = "keycloak";
    public const string RealmName = "box";
    public const string ClientId = "box-web";
    public const string ClientSecret = "box-web-secret";
    public const string TestUsername = "test";
    public const string TestPassword = "test";

    /// <summary>
    /// Docker/container memory limit. Without a limit, Keycloak's JVM treats Docker Desktop's
    /// full VM memory as available and sizes the heap to 70% of that.
    /// </summary>
    public const string ContainerMemoryLimit = "1g";

    /// <summary>
    /// Heap relative to the container limit (Keycloak's container defaults). Keep these in sync
    /// with <see cref="ContainerMemoryLimit"/> so the JVM does not claim the host's full RAM.
    /// </summary>
    public const string JavaOptsKcHeap = "-XX:MaxRAMPercentage=70 -XX:MinRAMPercentage=70 -XX:InitialRAMPercentage=50";

    public static string BuildAuthorityUri(string baseAddress) =>
        $"{baseAddress.TrimEnd('/')}/realms/{RealmName}";

    public static string BuildHealthCheckPath() => $"/realms/{RealmName}";
}
