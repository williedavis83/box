using BoxBottom.Aspire.Orchestration;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class DaprSidecarOrchestrationTests
{
    [Fact]
    public void ApplyDaprSidecarFromApiReferences_EnablesCallerAndCallee()
    {
        var stack = CreateStack("box");

        stack.ApplyDaprSidecarFromApiReferences();

        Assert.True(stack["primary-api"].EnableDaprSidecar);
        Assert.True(stack["secondary-api"].EnableDaprSidecar);
        Assert.False(stack["users-api"].EnableDaprSidecar);
    }

    [Fact]
    public void ApplyDaprSidecarFromApiReferences_DefaultStart_EnablesAllApis()
    {
        var stack = CreateStack("box");

        stack.ApplyDaprSidecarFromApiReferences(defaultStart: true);

        Assert.True(stack["primary-api"].EnableDaprSidecar);
        Assert.True(stack["secondary-api"].EnableDaprSidecar);
        Assert.True(stack["users-api"].EnableDaprSidecar);
    }

    [Fact]
    public void ApplyDaprSidecarFromApiReferences_Throws_WhenReferencedApiMissing()
    {
        var stack = CreateStack("box");
        stack["primary-api"].ApiReferences.Add("missing-api");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            stack.ApplyDaprSidecarFromApiReferences());

        Assert.Contains("missing-api", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ApiProjectOptions_CopyConstructor_PreservesEnableDaprSidecar()
    {
        var original = new ApiProjectOptions(
            logicalName: "primary-api",
            projectPath: "primary.csproj",
            enableDaprSidecar: true,
            apiReferences: ["secondary-api"]);

        var copy = new ApiProjectOptions(original);

        Assert.True(copy.EnableDaprSidecar);
        Assert.Equal(["secondary-api"], copy.ApiReferences);
    }

    private static StackDefinition CreateStack(string name)
    {
        var stack = new StackDefinition(
            name,
            web: new WebProjectOptions("web", "../BoxTop.Web"),
            edge: new EdgeProjectOptions("edge", @"..\BoxTop.Edge\BoxTop.Edge.csproj"),
            meta: new MetaProjectOptions("meta", @"..\BoxTop.Meta.Api\BoxTop.Meta.Api.csproj"));

        stack.AddApi(new ApiProjectOptions(
            logicalName: "primary-api",
            projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj",
            apiReferences: ["secondary-api"]));
        stack.AddApi(new ApiProjectOptions(
            logicalName: "secondary-api",
            projectPath: @"..\..\box-content\Foo.Secondary.Api\Foo.Secondary.Api.csproj",
            grpcOnlyAppChannel: true));
        stack.AddApi(new ApiProjectOptions(
            logicalName: "users-api",
            projectPath: @"..\..\box-top\BoxTop.Users.Api\BoxTop.Users.Api.csproj"));

        return stack;
    }
}
