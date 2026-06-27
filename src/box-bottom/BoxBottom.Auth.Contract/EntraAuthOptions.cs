namespace BoxBottom.Auth.Contract;

public sealed class EntraAuthOptions
{
    public const string SectionName = "Auth:Entra";

    public string TenantId { get; set; } = string.Empty;

    public string? Authority { get; set; }

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = "/api/signin-oidc";

    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";

    /// <summary>
    /// Browser origin where auth requests start and callbacks return (the web app URL in Aspire).
    /// </summary>
    public string? PublicOrigin { get; set; }

    /// <summary>
    /// Browser-facing callback path through edge (before YARP path transforms).
    /// </summary>
    public string ExternalCallbackPath { get; set; } = "/api/users/signin-oidc";
}
