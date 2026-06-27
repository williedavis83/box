namespace BoxBottom.Auth.Aspire;

public static class KeycloakOrchestrationConfiguration
{
    public const string ResourceName = "keycloak";
    public const string RealmName = "box";
    public const string ClientId = "box-web";
    public const string ClientSecret = "box-web-secret";
    public const string TestUsername = "test";
    public const string TestPassword = "test";

    public const string AuthorityEnvironmentVariable = "Auth__Entra__Authority";
    public const string TenantIdEnvironmentVariable = "Auth__Entra__TenantId";
    public const string ClientIdEnvironmentVariable = "Auth__Entra__ClientId";
    public const string ClientSecretEnvironmentVariable = "Auth__Entra__ClientSecret";
    public const string PublicOriginEnvironmentVariable = "Auth__Entra__PublicOrigin";

    public static string BuildAuthorityUri(string baseAddress) =>
        $"{baseAddress.TrimEnd('/')}/realms/{RealmName}";

    public static string BuildHealthCheckPath() => $"/realms/{RealmName}";
}
