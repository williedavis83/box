using BoxBottom.Aspire.Orchestration;
using BoxBottom.Auth.Aspire;
using BoxBottom.Auth.Emulation;
using BoxBottom.Auth.Entra;
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

    [Fact]
    public void BoeStackDerivedFromBox_DoesNotInheritEmulationEnvironmentVariables()
    {
        var box = CreateUsersStack("box");
        box["users-api"].EnvironmentVariables[EntraEmulationRegistryExtensions.EntraEmulationKey] =
            """{"singletons":{"Entra":{"provider":"Entra"}}}""";

        var boe = box with { Name = EntraProofOrchestrationConfiguration.BoeStackName };

        Assert.DoesNotContain(
            EntraEmulationRegistryExtensions.EntraEmulationKey,
            boe["users-api"].EnvironmentVariables.Keys);
    }

    [Fact]
    public void ConfigureBoeStack_SetsRealEntraEnvironmentVariables()
    {
        var boe = CreateUsersStack(EntraProofOrchestrationConfiguration.BoeStackName);

        EntraProofOrchestrator.ConfigureBoeStack(boe);

        var env = boe["users-api"].EnvironmentVariables;
        Assert.Equal(EntraAuthProvider.Name, env["Auth__Provider"]);
        Assert.Equal(
            EntraProofOrchestrationConfiguration.DefaultEntraAuthority,
            env["Auth__Entra__Authority"]);
        Assert.Equal(
            EntraProofOrchestrationConfiguration.DefaultEntraTenantId,
            env["Auth__Entra__TenantId"]);
        Assert.DoesNotContain(EntraEmulationRegistryExtensions.EntraEmulationKey, env.Keys);

        // ClientId and KeyVault URI are no longer injected as env vars; ClientId comes from
        // Key Vault (auth-entra-client-id) and the vault URI from appsettings.Development.json.
        Assert.DoesNotContain("Auth__Entra__ClientId", env.Keys);
        Assert.DoesNotContain("KeyVault__VaultUri", env.Keys);
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
