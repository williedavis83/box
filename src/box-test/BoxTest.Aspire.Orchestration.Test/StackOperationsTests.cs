using BoxBottom.Aspire.Orchestration;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class StackOperationsTests
{
    [Theory]
    [InlineData("box", "web", "BOX_WEB_HTTP")]
    [InlineData("box", "edge", "BOX_EDGE_HTTP")]
    [InlineData("box", "primary-api", "BOX_PRIMARY_API_HTTP")]
    [InlineData("bob", "edge", "BOB_EDGE_HTTP")]
    [InlineData("bob", "secondary-api", "BOB_SECONDARY_API_HTTP")]
    public void BuildHttpEnvironmentVariable_ProducesExpectedName(
        string stackName,
        string logicalName,
        string expected)
    {
        Assert.Equal(expected, StackOperations.BuildHttpEnvironmentVariable(stackName, logicalName));
    }

    [Fact]
    public void BuildHttpEnvironmentVariable_TwoStackLayout_ProducesEightUniquePlaywrightKeys()
    {
        var keys = new[]
        {
            StackOperations.BuildHttpEnvironmentVariable("box", "web"),
            StackOperations.BuildHttpEnvironmentVariable("box", "edge"),
            StackOperations.BuildHttpEnvironmentVariable("box", "primary-api"),
            StackOperations.BuildHttpEnvironmentVariable("box", "secondary-api"),
            StackOperations.BuildHttpEnvironmentVariable("bob", "web"),
            StackOperations.BuildHttpEnvironmentVariable("bob", "edge"),
            StackOperations.BuildHttpEnvironmentVariable("bob", "primary-api"),
            StackOperations.BuildHttpEnvironmentVariable("bob", "secondary-api"),
        };

        Assert.Equal(8, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("BOX_WEB_HTTP", keys[0]);
        Assert.Equal("BOB_SECONDARY_API_HTTP", keys[7]);
    }
}
