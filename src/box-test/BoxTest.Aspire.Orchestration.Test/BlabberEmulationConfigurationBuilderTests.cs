using Blabber.Emulator;
using Blabber.Emulator.Options;
using BoxBottom.Aspire.Orchestration;
using Foo.Primary.Shared.Blabber;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class BlabberEmulationConfigurationBuilderTests
{
    [Fact]
    public void BuildDocument_BobStackOverridesAllAnchorsWithDistinctSeedAccounts()
    {
        var orchestrator = new RecordingEmulationResourceOrchestrator();

        var builder = new BlabberEmulationConfigurationBuilder()
            .OverrideSingleton(BlabberKeys.Foo, "bob-foo")
            .OverrideList(BlabberKeys.ListA, "bob-foo", "bob-fee")
            .OverrideDictionary(
                BlabberKeys.DictA,
                ("bob-foo", "bob-foo"),
                ("bob-fee", "bob-fee"))
            .WithSeedAccounts(
                new BlabberEmulatorAccount
                {
                    Account = "bob-foo",
                    Bar = "bob-for",
                    Baz = "bob-foz",
                },
                new BlabberEmulatorAccount
                {
                    Account = "bob-fee",
                    Bar = "bob-fee-bar",
                    Baz = "bob-fee-baz",
                });

        builder.OrchestrateEmulationResources(orchestrator);
        var document = builder.BuildDocument();

        Assert.Equal("bob-foo", document.Singletons[BlabberKeys.Foo].Account);
        Assert.Equal(["bob-foo", "bob-fee"], document.Lists[BlabberKeys.ListA].Select(config => config.Account));
        Assert.Equal("bob-foo", document.Dictionaries[BlabberKeys.DictA]["bob-foo"].Account);
        Assert.Equal("bob-fee", document.Dictionaries[BlabberKeys.DictA]["bob-fee"].Account);
        Assert.Equal(new Uri("http://bleeb-emulator-api"), document.HostedServiceConfig!.Options.BleebEmulatorBaseUri);
        Assert.Equal(2, document.HostedServiceConfig.Options.Accounts.Count);
        Assert.Equal(1, orchestrator.OrchestrateSupportProjectCallCount);
    }

    [Fact]
    public void OrchestrateEmulationResources_ReusesExistingEmulatorResource()
    {
        var orchestrator = new RecordingEmulationResourceOrchestrator();

        var firstBuilder = new BlabberEmulationConfigurationBuilder()
            .OverrideSingleton(BlabberKeys.Foo, "bob-foo");
        firstBuilder.OrchestrateEmulationResources(orchestrator);

        var secondBuilder = new BlabberEmulationConfigurationBuilder()
            .OverrideSingleton(BlabberKeys.Foo, "bob-foo");
        secondBuilder.OrchestrateEmulationResources(orchestrator);

        Assert.Equal(1, orchestrator.OrchestrateSupportProjectCallCount);
    }

    private sealed class RecordingEmulationResourceOrchestrator : BoxBottom.Emulation.Shared.IEmulationResourceOrchestrator
    {
        private readonly Dictionary<string, BoxBottom.Emulation.Shared.EmulationOrchestratedResource> _resources =
            new(StringComparer.OrdinalIgnoreCase);

        public int OrchestrateSupportProjectCallCount { get; private set; }

        public bool TryGetOrchestratedResource(
            string resourceName,
            out BoxBottom.Emulation.Shared.EmulationOrchestratedResource resource) =>
            _resources.TryGetValue(resourceName, out resource!);

        public BoxBottom.Emulation.Shared.EmulationOrchestratedResource OrchestrateSupportProject(
            string resourceName,
            string projectPath)
        {
            OrchestrateSupportProjectCallCount++;

            if (_resources.TryGetValue(resourceName, out var existing))
            {
                return existing;
            }

            var resource = new BoxBottom.Emulation.Shared.EmulationOrchestratedResource(
                resourceName,
                EdgeRoutingConfiguration.BuildApiClusterAddress(resourceName));

            _resources[resourceName] = resource;
            return resource;
        }
    }
}
