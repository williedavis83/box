using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace BoxTop.Users.Api.Configuration;

internal sealed class BoxKeyVaultSecretManager : KeyVaultSecretManager
{
    public override bool Load(SecretProperties properties) =>
        properties.Name.StartsWith("auth-entra-", StringComparison.OrdinalIgnoreCase);

    public override string GetKey(KeyVaultSecret secret) =>
        secret.Name.ToLowerInvariant() switch
        {
            "auth-entra-client-secret" => "Auth:Entra:ClientSecret",
            "auth-entra-client-id" => "Auth:Entra:ClientId",
            "auth-entra-authority" => "Auth:Entra:Authority",
            _ => secret.Name,
        };
}
