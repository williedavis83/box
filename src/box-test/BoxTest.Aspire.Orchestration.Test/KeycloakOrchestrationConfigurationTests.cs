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
}
