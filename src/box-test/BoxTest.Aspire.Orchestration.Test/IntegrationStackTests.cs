using BoxBottom.Aspire.Orchestration;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class IntegrationStackTests
{
    [Fact]
    public void AsIntegrationStack_MarksDefinition()
    {
        var stack = CreateStack("bob");

        stack.AsIntegrationStack();

        Assert.True(stack.IsIntegrationStack);
    }

    [Fact]
    public void WithExpression_DoesNotCopyIntegrationFlag_UntilMarked()
    {
        var box = CreateStack("box");
        box.AsIntegrationStack();

        var bob = box with { Name = "bob" };

        Assert.True(box.IsIntegrationStack);
        Assert.True(bob.IsIntegrationStack);

        var plain = CreateStack("box");
        var cloned = plain with { Name = "bob" };
        Assert.False(cloned.IsIntegrationStack);

        cloned.AsIntegrationStack();
        Assert.True(cloned.IsIntegrationStack);
        Assert.False(plain.IsIntegrationStack);
    }

    private static StackDefinition CreateStack(string name)
    {
        var stack = new StackDefinition(name,
            web: new WebProjectOptions("web", "../BoxTop.Web"),
            edge: new EdgeProjectOptions("edge", @"..\BoxTop.Edge\BoxTop.Edge.csproj"),
            meta: new MetaProjectOptions("meta", @"..\BoxTop.Meta.Api\BoxTop.Meta.Api.csproj"));

        stack.AddApi(new ApiProjectOptions(
            logicalName: "primary-api",
            projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj"));

        return stack;
    }
}
