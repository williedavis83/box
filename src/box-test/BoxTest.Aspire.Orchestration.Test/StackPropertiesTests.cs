using BoxBottom.Aspire.ServiceDefaults;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class StackPropertiesTests
{
    [Theory]
    [InlineData("box", "secondary-api", "box-secondary-api")]
    [InlineData("bob", "primary-api", "bob-primary-api")]
    [InlineData("box", "primary-api", "box-primary-api")]
    public void ResolveDaprAppId_WithStackPrefix_ReturnsPrefixedAppId(
        string stackName,
        string logicalName,
        string expected)
    {
        var properties = new StackProperties(stackName);

        Assert.Equal(expected, properties.ResolveDaprAppId(logicalName));
    }

    [Fact]
    public void ResolveDaprAppId_WithoutStackPrefix_ReturnsLogicalName()
    {
        var properties = new StackProperties(string.Empty);

        Assert.Equal("secondary-api", properties.ResolveDaprAppId("secondary-api"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveDaprAppId_RejectsBlankLogicalName(string logicalName)
    {
        var properties = new StackProperties("box");

        Assert.Throws<ArgumentException>(() => properties.ResolveDaprAppId(logicalName));
    }
}
