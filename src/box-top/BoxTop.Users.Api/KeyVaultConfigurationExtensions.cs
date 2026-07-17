using Azure.Identity;
using BoxTop.Users.Api.Configuration;

namespace BoxTop.Users.Api;

internal static class KeyVaultConfigurationExtensions
{
    public static WebApplicationBuilder AddBoxKeyVaultConfiguration(this WebApplicationBuilder builder)
    {
        var authProvider = builder.Configuration["Auth:Provider"];
        if (!string.Equals(authProvider, "Entra", StringComparison.OrdinalIgnoreCase))
        {
            return builder;
        }

        var vaultUri = builder.Configuration["KeyVault:VaultUri"];
        if (string.IsNullOrWhiteSpace(vaultUri))
        {
            return builder;
        }

        builder.Configuration.AddAzureKeyVault(
            new Uri(vaultUri),
            new DefaultAzureCredential(),
            new BoxKeyVaultSecretManager());

        return builder;
    }
}
