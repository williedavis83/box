using BoxBottom.Aspire.Orchestration;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class OrchestratorShapeTests
{
    [Fact]
    public void WithExpression_BoxAndBobStacks_HaveDistinctEdgeResourceNames()
    {
        var box = CreateOrchestratorShapeStack("box");
        var bob = box with { Name = "bob" };

        Assert.Equal("box-edge", box.Edge.StackName);
        Assert.Equal("bob-edge", bob.Edge.StackName);
        Assert.Equal("box", box.Edge.StackPrefix);
        Assert.Equal("bob", bob.Edge.StackPrefix);
    }

    [Fact]
    public void WithExpression_BoxAndBobStacks_HaveDistinctPrimaryApiClusterTargets()
    {
        var box = CreateOrchestratorShapeStack("box");
        var bob = box with { Name = "bob" };

        var boxCluster = EdgeRoutingConfiguration.BuildApiClusterAddress(box["primary-api"].StackName);
        var bobCluster = EdgeRoutingConfiguration.BuildApiClusterAddress(bob["primary-api"].StackName);

        Assert.Equal("http://box-primary-api", boxCluster);
        Assert.Equal("http://bob-primary-api", bobCluster);
        Assert.NotEqual(boxCluster, bobCluster);
    }

    [Fact]
    public void WithExpression_BoxStack_UnchangedAfterBobClone()
    {
        var box = CreateOrchestratorShapeStack("box");
        var bob = box with { Name = "bob" };

        bob["secondary-api"].EnvironmentVariables["World__Message"] = "Bob";

        Assert.Equal("box-edge", box.Edge.StackName);
        Assert.Equal("box-primary-api", box["primary-api"].StackName);
        Assert.Empty(box["secondary-api"].EnvironmentVariables);
        Assert.Equal("Bob", bob["secondary-api"].EnvironmentVariables["World__Message"]);
    }

    [Fact]
    public void OrchestratorPattern_BobClone_StartsWithEmptySecondaryEnvBeforeMutation()
    {
        var box = CreateOrchestratorShapeStack("box");
        var bob = box with { Name = "bob" };

        Assert.Empty(box["secondary-api"].EnvironmentVariables);
        Assert.Empty(bob["secondary-api"].EnvironmentVariables);

        bob["secondary-api"].EnvironmentVariables["World__Message"] = "Bob";

        Assert.Empty(box["secondary-api"].EnvironmentVariables);
        Assert.Equal("Bob", bob["secondary-api"].EnvironmentVariables["World__Message"]);
    }

    [Fact]
    public void DefaultPrimaryRoute_WouldProxyStackNameEndpointToPrimaryApi()
    {
        Assert.True(EdgeRoutingConfiguration.DefaultPrimaryRouteMatchesPath("/api/StackName"));
        Assert.Equal("/api/{**catch-all}", EdgeRoutingConfiguration.BuildDefaultPrimaryRoutePath());
    }

    private static StackDefinition CreateOrchestratorShapeStack(string name)
    {
        var stack = new StackDefinition(name,
            web: new WebProjectOptions(
                logicalName: "web",
                projectPath: "../BoxTop.Web"),
            edge: new EdgeProjectOptions(
                logicalName: "edge",
                projectPath: @"..\BoxTop.Edge\BoxTop.Edge.csproj"),
            meta: new MetaProjectOptions(
                logicalName: "meta",
                projectPath: @"..\BoxTop.Meta.Api\BoxTop.Meta.Api.csproj"));

        stack.AddApi(new ApiProjectOptions(
            logicalName: "primary-api",
            projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj",
            apiReferences: ["secondary-api"]));

        stack.AddApi(new ApiProjectOptions(
            logicalName: "secondary-api",
            projectPath: @"..\..\box-content\Foo.Secondary.Api\Foo.Secondary.Api.csproj",
            grpcOnlyAppChannel: true));

        return stack;
    }
}
