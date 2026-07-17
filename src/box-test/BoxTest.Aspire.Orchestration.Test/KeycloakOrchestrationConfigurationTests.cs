using System.Text.Json;
using BoxBottom.Auth.Aspire;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class KeycloakOrchestrationConfigurationTests
{
    [Fact]
    public void BuildAuthorityUri_IncludesRealmName()
    {
        var authority = KeycloakOrchestrationConfiguration.BuildAuthorityUri("http://localhost:8080");

        Assert.Equal("http://localhost:8080/realms/box", authority);
    }

    [Fact]
    public void BuildHealthCheckPath_UsesRealmEndpoint()
    {
        Assert.Equal("/realms/box", KeycloakOrchestrationConfiguration.BuildHealthCheckPath());
    }

    [Fact]
    public void ContainerMemoryLimit_IsConfigured()
    {
        Assert.Equal("1g", KeycloakOrchestrationConfiguration.ContainerMemoryLimit);
        Assert.Contains("MaxRAMPercentage=70", KeycloakOrchestrationConfiguration.JavaOptsKcHeap, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildAuthEmulationJson_SerializesEntraDocument()
    {
        var json = KeycloakOrchestrator.BuildAuthEmulationJson(
            "http://localhost:8080",
            "http://localhost:49240");

        using var document = JsonDocument.Parse(json);
        var entra = document.RootElement.GetProperty("singletons").GetProperty("Entra");

        Assert.Equal("Entra", entra.GetProperty("provider").GetString());
        Assert.Equal("box", entra.GetProperty("tenantId").GetString());
        Assert.Equal("http://localhost:8080/realms/box", entra.GetProperty("authority").GetString());
        Assert.Equal("box-web", entra.GetProperty("clientId").GetString());
        Assert.Equal("box-web-secret", entra.GetProperty("clientSecret").GetString());
        Assert.Equal("http://localhost:49240", entra.GetProperty("publicOrigin").GetString());
        Assert.False(document.RootElement.TryGetProperty("lists", out _));
        Assert.False(document.RootElement.TryGetProperty("dictionaries", out _));
    }
}
