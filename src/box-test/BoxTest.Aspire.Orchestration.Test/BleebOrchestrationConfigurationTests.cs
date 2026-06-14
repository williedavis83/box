using Bleeb.Aspire;
using Xunit;
namespace BoxTest.Aspire.Orchestration.Test;

public class BleebOrchestrationConfigurationTests
{
    [Fact]
    public void BuildBleebServiceAddress_UsesAspireServiceDiscoveryUri()
    {
        Assert.Equal(
            "http://bleeb-api",
            BleebOrchestrationConfiguration.BuildBleebServiceAddress());
    }

    [Fact]
    public void BaseUriEnvironmentVariable_MapsToPrimaryApiBleebConfiguration()
    {
        Assert.Equal("Bleeb__BaseUri", BleebOrchestrationConfiguration.BaseUriEnvironmentVariable);
    }

    [Fact]
    public void ApiProjectPath_ResolvesFromAppHostDirectory()
    {
        var appHostDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "box-top", "BoxTop.Aspire"));

        var projectPath = Path.GetFullPath(
            Path.Combine(appHostDirectory, BleebOrchestrationConfiguration.ApiProjectPath));

        Assert.True(File.Exists(projectPath), $"Expected Bleeb.Api project at '{projectPath}'.");
    }
}
