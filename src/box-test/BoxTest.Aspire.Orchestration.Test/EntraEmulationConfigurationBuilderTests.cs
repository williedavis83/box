using BoxBottom.Auth.Emulation;
using BoxBottom.Emulation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class EntraEmulationConfigurationBuilderTests
{
    [Fact]
    public void BuildDocument_IncludesEntraOverride()
    {
        var document = new EntraEmulationConfigurationBuilder()
            .OverrideEntra()
            .BuildDocument();

        Assert.Equal(EntraEmulationRegistryExtensions.EntraEmulationKey, new EntraEmulationConfigurationBuilder().EmulationKey);
        Assert.True(document.Singletons.ContainsKey("Entra"));
        Assert.Equal("Entra", document.Singletons["Entra"].Provider);
    }

    [Fact]
    public void ApplyConfiguredEmulation_OverridesEntraConfigurationAfterExistingSources()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auth:Entra:Authority"] = "https://real-entra.example/",
            ["Auth:Entra:ClientId"] = "real-client",
            [EntraEmulationRegistryExtensions.EntraEmulationKey] =
                """
                {
                  "singletons": {
                    "Entra": {
                      "provider": "Entra",
                      "tenantId": "box",
                      "authority": "http://keycloak/realms/box",
                      "clientId": "box-web",
                      "clientSecret": "box-web-secret",
                      "publicOrigin": "http://box-web"
                    }
                  }
                }
                """,
        });

        builder.AddEmulation()
            .RegisterEntraEmulation()
            .ApplyConfiguredEmulation();

        Assert.Equal("Entra", builder.Configuration["Auth:Provider"]);
        Assert.Equal("box", builder.Configuration["Auth:Entra:TenantId"]);
        Assert.Equal("http://keycloak/realms/box", builder.Configuration["Auth:Entra:Authority"]);
        Assert.Equal("box-web", builder.Configuration["Auth:Entra:ClientId"]);
        Assert.Equal("box-web-secret", builder.Configuration["Auth:Entra:ClientSecret"]);
        Assert.Equal("http://box-web", builder.Configuration["Auth:Entra:PublicOrigin"]);
    }
}
