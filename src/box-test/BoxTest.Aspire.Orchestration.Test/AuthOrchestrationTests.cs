using BoxBottom.Aspire.Orchestration;
using BoxBottom.Auth.Emulation;
using BoxBottom.Auth.ZeroAuth;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AuthOrchestrationTests
{
    [Fact]
    public void OrchestratorPattern_BoxUsesEntraEmulationAndBobUsesZeroAuth()
    {
        var box = CreateUsersStack("box");
        var bob = box with { Name = "bob" };

        box["users-api"].EnvironmentVariables[EntraEmulationRegistryExtensions.EntraEmulationKey] =
            """{"singletons":{"Entra":{"provider":"Entra"}}}""";

        bob["users-api"].EnvironmentVariables["Auth__Provider"] = ZeroAuthAuthProvider.Name;

        Assert.Contains(
            EntraEmulationRegistryExtensions.EntraEmulationKey,
            box["users-api"].EnvironmentVariables.Keys);
        Assert.Equal(ZeroAuthAuthProvider.Name, bob["users-api"].EnvironmentVariables["Auth__Provider"]);
    }

    private static StackDefinition CreateUsersStack(string name)
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

        stack.AddApi(new ApiProjectOptions(
            logicalName: "users-api",
            projectPath: @"..\..\box-top\BoxTop.Users.Api\BoxTop.Users.Api.csproj"));

        return stack;
    }
}
