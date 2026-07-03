namespace BoxBottom.Auth.Aspire;

public static class EntraProofOrchestrationConfiguration
{
    public const string BoeStackName = "boe";
    public const string KeyVaultUriEnvironmentVariable = "KeyVault__VaultUri";
    public const string AuthProviderEnvironmentVariable = "Auth__Provider";

    public const string DefaultEntraAuthority =
        "https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0";

    public const string DefaultEntraTenantId = "9af8af7b-10ee-4bd5-b71c-20daa8e37878";
}
