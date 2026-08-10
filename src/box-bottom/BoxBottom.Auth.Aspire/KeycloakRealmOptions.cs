namespace BoxBottom.Auth.Aspire;

/// <summary>
/// One identity sphere inside the shared Keycloak container. Aspire imports every
/// registered realm JSON; stacks bind to a specific realm via <see cref="KeycloakStackBinding"/>.
/// </summary>
public sealed record KeycloakRealmOptions
{
    public required string RealmName { get; init; }

    public required string ClientId { get; init; }

    public required string ClientSecret { get; init; }

    /// <summary>File name under the Keycloak import directory (e.g. <c>box-realm.json</c>).</summary>
    public required string ImportFileName { get; init; }

    public string TestUsername { get; init; } = "test";

    public string TestPassword { get; init; } = "test";
}

/// <summary>
/// Binds an Aspire stack to a Keycloak realm. <see cref="PublicOrigin"/> for that stack
/// always comes from the stack's web HTTP endpoint URL.
/// </summary>
public sealed record KeycloakStackBinding(string StackName, KeycloakRealmOptions Realm);
