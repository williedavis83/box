using BoxBottom.AzureTable.Emulation;
using Foo.Primary.Shared.AzureTable;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AzureTableEmulationConfigurationBuilderTests
{
    [Fact]
    public void BuildDocument_OverrideSingleton_AddsApplicationAnchor()
    {
        const string anchorName = "custom-azure-table";

        var document = new AzureTableEmulationConfigurationBuilder()
            .OverrideSingleton(anchorName)
            .BuildDocument();

        Assert.True(document.Singletons.ContainsKey(anchorName));
    }

    [Fact]
    public void BuildDocument_OverrideDictionary_AddsMemberAnchorKeys()
    {
        var document = new AzureTableEmulationConfigurationBuilder()
            .OverrideDictionary(
                AzureTableKeys.GeoReplicas,
                (AzureTableGeoKeys.UsEast, AzureTableKeys.GeoUsEast),
                (AzureTableGeoKeys.EuWest, AzureTableKeys.GeoEuWest))
            .BuildDocument();

        Assert.Equal(AzureTableKeys.GeoUsEast, document.Dictionaries[AzureTableKeys.GeoReplicas][AzureTableGeoKeys.UsEast].MemberAnchorKey);
        Assert.Equal(AzureTableKeys.GeoEuWest, document.Dictionaries[AzureTableKeys.GeoReplicas][AzureTableGeoKeys.EuWest].MemberAnchorKey);
    }

    [Fact]
    public void OrchestrateEmulationResources_DoesNotTouchGeneralEmulationOrchestrator()
    {
        var orchestrator = new RecordingEmulationResourceOrchestrator();

        new AzureTableEmulationConfigurationBuilder()
            .OverrideSingleton("azure-table")
            .OrchestrateEmulationResources(orchestrator);

        Assert.Equal(0, orchestrator.OrchestrateSupportProjectCallCount);
    }

    [Fact]
    public void BuildDocument_BobScenarioShape_MatchesExpectedOverrides()
    {
        var document = new AzureTableEmulationConfigurationBuilder()
            .OverrideSingleton(AzureTableKeys.Orders)
            .OverrideSingleton(AzureTableKeys.Analytics)
            .OverrideSingleton(AzureTableKeys.GeoUsEast)
            .OverrideSingleton(AzureTableKeys.GeoEuWest)
            .OverrideDictionary(
                AzureTableKeys.GeoReplicas,
                (AzureTableGeoKeys.UsEast, AzureTableKeys.GeoUsEast),
                (AzureTableGeoKeys.EuWest, AzureTableKeys.GeoEuWest))
            .BuildDocument();

        Assert.Equal(4, document.Singletons.Count);
        Assert.Equal(2, document.Dictionaries[AzureTableKeys.GeoReplicas].Count);
    }

    private sealed class RecordingEmulationResourceOrchestrator : BoxBottom.Emulation.Shared.IEmulationResourceOrchestrator
    {
        public int OrchestrateSupportProjectCallCount { get; private set; }

        public bool TryGetOrchestratedResource(
            string resourceName,
            out BoxBottom.Emulation.Shared.EmulationOrchestratedResource resource)
        {
            resource = null!;
            return false;
        }

        public BoxBottom.Emulation.Shared.EmulationOrchestratedResource OrchestrateSupportProject(
            string resourceName,
            string projectPath)
        {
            OrchestrateSupportProjectCallCount++;
            return new BoxBottom.Emulation.Shared.EmulationOrchestratedResource(
                resourceName,
                $"http://{resourceName}");
        }
    }
}
