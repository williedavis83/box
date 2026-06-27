using BoxBottom.Auth.Emulation;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class EntraEmulationConfigurationBuilderTests
{
    [Fact]
    public void BuildDocument_IncludesEntraOverride()
    {
        var document = new EntraEmulationConfigurationBuilder()
            .OverrideEntra()
            .BuildDocument();

        Assert.Equal(EntraEmulationRegistryExtensions.EntraEmulationKey, new EntraEmulationConfigurationBuilder().EmulationKey);
        Assert.True(document.Singletons.ContainsKey("Entra"));
        Assert.Equal("Entra", document.Singletons["Entra"].Provider);
    }
}
