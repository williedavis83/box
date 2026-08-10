namespace BoxBottom.Auth.Aspire;

public static class KeycloakOrchestrationConfiguration
{
    public const string ResourceName = "keycloak";

    /// <summary>Default / legacy realm name (same as <see cref="Box"/>).</summary>
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

    /// <summary>Primary local-dev realm used by the <c>box</c> stack.</summary>
    public static KeycloakRealmOptions Box { get; } = new()
    {
        RealmName = RealmName,
        ClientId = ClientId,
        ClientSecret = ClientSecret,
        ImportFileName = "box-realm.json",
        TestUsername = TestUsername,
        TestPassword = TestPassword,
    };

    /// <summary>
    /// Second realm co-hosted in the same Keycloak instance. Ready for another stack that
    /// wants Keycloak Entra emulation with an isolated identity sphere (e.g. a second FE
    /// shell from multi-frontend work). Not wired to <c>bob</c>/<c>boe</c>.
    /// </summary>
    public static KeycloakRealmOptions Bok { get; } = new()
    {
        RealmName = "bok",
        ClientId = "bok-web",
        ClientSecret = "bok-web-secret",
        ImportFileName = "bok-realm.json",
        TestUsername = TestUsername,
        TestPassword = TestPassword,
    };

    /// <summary>All realms imported into the shared Keycloak container on startup.</summary>
    public static IReadOnlyList<KeycloakRealmOptions> ImportedRealms { get; } = [Box, Bok];

    public static KeycloakRealmOptions GetRealm(string realmName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(realmName);

        foreach (var realm in ImportedRealms)
        {
            if (string.Equals(realm.RealmName, realmName, StringComparison.OrdinalIgnoreCase))
            {
                return realm;
            }
        }

        throw new KeyNotFoundException(
            $"No Keycloak realm named '{realmName}' is registered. Known realms: {string.Join(", ", ImportedRealms.Select(r => r.RealmName))}.");
    }

    public static string BuildAuthorityUri(string baseAddress, string? realmName = null) =>
        $"{baseAddress.TrimEnd('/')}/realms/{realmName ?? RealmName}";

    public static string BuildAuthorityUri(string baseAddress, KeycloakRealmOptions realm)
    {
        ArgumentNullException.ThrowIfNull(realm);
        return BuildAuthorityUri(baseAddress, realm.RealmName);
    }

    /// <summary>Health check hits the default realm; any imported realm proves Keycloak is up.</summary>
    public static string BuildHealthCheckPath(string? realmName = null) =>
        $"/realms/{realmName ?? RealmName}";
}
