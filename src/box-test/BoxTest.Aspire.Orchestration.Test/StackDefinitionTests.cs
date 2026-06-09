using BoxBottom.Aspire.Orchestration;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class StackDefinitionTests
{
    [Fact]
    public void WithExpression_CreatesDistinctStackInstance()
    {
        var stack = CreateStack("box");

        var renamed = stack with { Name = "name2" };

        Assert.NotSame(stack, renamed);
    }

    [Fact]
    public void WithExpression_RenamesWebEdgeMetaAndApis()
    {
        var stack = CreateStack("box");

        var renamed = stack with { Name = "name2" };

        Assert.Equal("name2", renamed.Name);
        Assert.Equal("name2", renamed.Web.StackPrefix);
        Assert.Equal("name2-web", renamed.Web.StackName);
        Assert.Equal("name2", renamed.Edge.StackPrefix);
        Assert.Equal("name2-edge", renamed.Edge.StackName);
        Assert.Equal("name2", renamed.Meta.StackPrefix);
        Assert.Equal("name2-meta", renamed.Meta.StackName);
        Assert.Equal("name2-primary-api", renamed["primary-api"].StackName);
        Assert.Equal("name2-secondary-api", renamed["secondary-api"].StackName);
    }

    [Fact]
    public void WithExpression_LeavesOriginalStackUnchanged()
    {
        var stack = CreateStack("box");

        _ = stack with { Name = "name2" };

        Assert.Equal("box", stack.Name);
        Assert.Equal("box", stack.Web.StackPrefix);
        Assert.Equal("box-web", stack.Web.StackName);
        Assert.Equal("box-edge", stack.Edge.StackName);
        Assert.Equal("box-meta", stack.Meta.StackName);
        Assert.Equal("box-primary-api", stack["primary-api"].StackName);
        Assert.Equal("box-secondary-api", stack["secondary-api"].StackName);
    }

    [Fact]
    public void WithExpression_CreatesDistinctApiDictionary()
    {
        var stack = CreateStack("box");

        var renamed = stack with { Name = "name2" };

        renamed.AddApi(new ApiProjectOptions(
            logicalName: "tertiary-api",
            projectPath: @"..\..\box-content\Foo.Tertiary.Api\Foo.Tertiary.Api.csproj"));

        Assert.Equal(2, stack.GetApis().Count());
        Assert.Equal(3, renamed.GetApis().Count());
        Assert.Equal("name2-tertiary-api", renamed["tertiary-api"].StackName);
        Assert.Throws<KeyNotFoundException>(() => stack["tertiary-api"]);
    }

    [Fact]
    public void WithExpression_WithSameNameStillCreatesNewInstance()
    {
        var stack = CreateStack("box");

        var cloned = stack with { Name = "box" };

        Assert.NotSame(stack, cloned);
        Assert.Equal("box", cloned.Name);
        Assert.Equal("box-web", cloned.Web.StackName);
    }

    [Fact]
    public void WithExpression_ClearsWebEnvironmentVariables()
    {
        var stack = CreateStackWithEnvironmentVariables("box");
        var renamed = stack with { Name = "name2" };

        Assert.NotSame(stack.Web.EnvironmentVariables, renamed.Web.EnvironmentVariables);
        Assert.Empty(renamed.Web.EnvironmentVariables);

        renamed.Web.EnvironmentVariables["BOX_WEB_HTTP"] = "name2-value";

        Assert.Equal("box-value", stack.Web.EnvironmentVariables["BOX_WEB_HTTP"]);
        Assert.Equal("name2-value", renamed.Web.EnvironmentVariables["BOX_WEB_HTTP"]);
    }

    [Fact]
    public void WithExpression_ClearsEdgeEnvironmentVariables()
    {
        var stack = CreateStackWithEnvironmentVariables("box");
        var renamed = stack with { Name = "name2" };

        Assert.NotSame(stack.Edge.EnvironmentVariables, renamed.Edge.EnvironmentVariables);
        Assert.Empty(renamed.Edge.EnvironmentVariables);

        renamed.Edge.EnvironmentVariables["BOX_EDGE_HTTP"] = "name2-value";

        Assert.Equal("box-value", stack.Edge.EnvironmentVariables["BOX_EDGE_HTTP"]);
        Assert.Equal("name2-value", renamed.Edge.EnvironmentVariables["BOX_EDGE_HTTP"]);
    }

    [Fact]
    public void WithExpression_ClearsMetaEnvironmentVariables()
    {
        var stack = CreateStackWithEnvironmentVariables("box");
        var renamed = stack with { Name = "name2" };

        Assert.NotSame(stack.Meta.EnvironmentVariables, renamed.Meta.EnvironmentVariables);
        Assert.Empty(renamed.Meta.EnvironmentVariables);

        renamed.Meta.EnvironmentVariables["BOX_META_HTTP"] = "name2-value";

        Assert.Equal("box-value", stack.Meta.EnvironmentVariables["BOX_META_HTTP"]);
        Assert.Equal("name2-value", renamed.Meta.EnvironmentVariables["BOX_META_HTTP"]);
    }

    [Fact]
    public void WithExpression_ClearsApiEnvironmentVariables()
    {
        var stack = CreateStackWithEnvironmentVariables("box");
        var renamed = stack with { Name = "name2" };

        var originalPrimary = stack["primary-api"];
        var renamedPrimary = renamed["primary-api"];

        Assert.NotSame(originalPrimary.EnvironmentVariables, renamedPrimary.EnvironmentVariables);
        Assert.Empty(renamedPrimary.EnvironmentVariables);

        renamedPrimary.EnvironmentVariables["CUSTOM_SETTING"] = "name2-value";

        Assert.Equal("box-value", originalPrimary.EnvironmentVariables["CUSTOM_SETTING"]);
        Assert.Equal("name2-value", renamedPrimary.EnvironmentVariables["CUSTOM_SETTING"]);
    }

    [Fact]
    public void WithExpression_OriginalEnvironmentVariableChanges_DoNotAffectClone()
    {
        var stack = CreateStackWithEnvironmentVariables("box");
        var renamed = stack with { Name = "name2" };

        stack.Web.EnvironmentVariables["BOX_WEB_HTTP"] = "mutated-box-value";
        stack["secondary-api"].EnvironmentVariables["CUSTOM_SETTING"] = "mutated-box-value";

        Assert.Empty(renamed.Web.EnvironmentVariables);
        Assert.Empty(renamed["secondary-api"].EnvironmentVariables);
    }

    private static StackDefinition CreateStack(string name)
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
                projectPath: @"..\BoxTop.Meta.Api\BoxTop.Meta.Api.csproj")
        );

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

    private static StackDefinition CreateStackWithEnvironmentVariables(string name)
    {
        var stack = new StackDefinition(name,
            web: new WebProjectOptions(
                logicalName: "web",
                projectPath: "../BoxTop.Web",
                environmentVariables: new Dictionary<string, string>
                {
                    ["BOX_WEB_HTTP"] = "box-value",
                }),
            edge: new EdgeProjectOptions(
                logicalName: "edge",
                projectPath: @"..\BoxTop.Edge\BoxTop.Edge.csproj",
                environmentVariables: new Dictionary<string, string>
                {
                    ["BOX_EDGE_HTTP"] = "box-value",
                }),
            meta: new MetaProjectOptions(
                logicalName: "meta",
                projectPath: @"..\BoxTop.Meta.Api\BoxTop.Meta.Api.csproj",
                environmentVariables: new Dictionary<string, string>
                {
                    ["BOX_META_HTTP"] = "box-value",
                }));

        stack.AddApi(new ApiProjectOptions(
            logicalName: "primary-api",
            projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj",
            environmentVariables: new Dictionary<string, string>
            {
                ["CUSTOM_SETTING"] = "box-value",
            }));

        stack.AddApi(new ApiProjectOptions(
            logicalName: "secondary-api",
            projectPath: @"..\..\box-content\Foo.Secondary.Api\Foo.Secondary.Api.csproj",
            grpcOnlyAppChannel: true,
            environmentVariables: new Dictionary<string, string>
            {
                ["CUSTOM_SETTING"] = "box-value",
            }));

        return stack;
    }
}
