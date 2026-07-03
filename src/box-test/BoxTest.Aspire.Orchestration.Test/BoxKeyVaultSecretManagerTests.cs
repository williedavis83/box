using Azure.Security.KeyVault.Secrets;
using BoxTop.Users.Api.Configuration;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class BoxKeyVaultSecretManagerTests
{
    [Theory]
    [InlineData("auth-entra-client-secret", "Auth:Entra:ClientSecret")]
    [InlineData("auth-entra-client-id", "Auth:Entra:ClientId")]
    [InlineData("auth-entra-authority", "Auth:Entra:Authority")]
    public void GetKey_MapsEntraSecretsToConfigurationKeys(string secretName, string expectedKey)
    {
        var manager = new BoxKeyVaultSecretManager();
        var secret = new KeyVaultSecret(secretName, "value");

        Assert.Equal(expectedKey, manager.GetKey(secret));
    }

    [Fact]
    public void Load_IncludesAuthEntraSecretsOnly()
    {
        var manager = new BoxKeyVaultSecretManager();
        var properties = new SecretProperties("auth-entra-client-secret");

        Assert.True(manager.Load(properties));
    }
}
