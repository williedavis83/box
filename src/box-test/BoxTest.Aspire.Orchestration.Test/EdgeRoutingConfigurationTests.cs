using BoxBottom.Aspire.Orchestration;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class EdgeRoutingConfigurationTests
{
    [Fact]
    public void BuildRoutePath_PrimaryApi_UsesDefaultCatchAll()
    {
        Assert.Equal(
            "/api/{**catch-all}",
            EdgeRoutingConfiguration.BuildRoutePath("primary-api", apiCount: 2));
    }

    [Fact]
    public void BuildRoutePath_SecondaryApi_UsesStackScopedPrefix()
    {
        Assert.Equal(
            "/api/secondary/{**catch-all}",
            EdgeRoutingConfiguration.BuildRoutePath("secondary-api", apiCount: 2));
    }

    [Fact]
    public void BuildRoutePathPrefix_SecondaryApi_StripsApiSuffix()
    {
        Assert.Equal("/api/secondary", EdgeRoutingConfiguration.BuildRoutePathPrefix("secondary-api"));
    }

    [Fact]
    public void BuildMetaRoutePath_UsesMetaPrefix()
    {
        Assert.Equal("/api/meta/{**catch-all}", EdgeRoutingConfiguration.BuildMetaRoutePath());
        Assert.Equal("/api/meta", EdgeRoutingConfiguration.BuildMetaRoutePathPrefix());
    }

    [Theory]
    [InlineData("primary_api", "primary-api")]
    [InlineData("Secondary-API", "secondary-api")]
    public void BuildRouteKey_NormalizesLogicalName(string logicalName, string expected)
    {
        Assert.Equal(expected, EdgeRoutingConfiguration.BuildRouteKey(logicalName));
    }
}
