using Bleeb.Aspire;
using Blabber.Emulator;
using BoxBottom.Aspire.Orchestration;
using Foo.Primary.Shared.Blabber;

namespace BoxPack.Aspire.Orchestration;

public static class Orchestrator
{
    private const string BoxStackName = "box";
    private const string BobStackName = "bob";
    private const string WorldMessageEnvironmentVariable = "World__Message";

    public static IReadOnlyDictionary<string, StackResources> Orchestrate(StackOperations stackOperations)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);

        var boxStack = stackOperations.CreateStackDefinition(BoxStackName);

        boxStack.AddApi(new ApiProjectOptions(
            logicalName: "primary-api",
            projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj",
            apiReferences: ["secondary-api"]));

        boxStack.AddApi(new ApiProjectOptions(
            logicalName: "secondary-api",
            projectPath: @"..\..\box-content\Foo.Secondary.Api\Foo.Secondary.Api.csproj",
            grpcOnlyAppChannel: true));

        var bobStack = boxStack with { Name = BobStackName };
        bobStack["secondary-api"].EnvironmentVariables[WorldMessageEnvironmentVariable] = "Bob";

        var bobBlabberEmulation = new BlabberEmulationConfigurationBuilder()
            .OverrideSingleton(BlabberKeys.Foo, "foo")
            .OverrideList(BlabberKeys.ListA, "foo", "bob-fee")
            .OverrideDictionary(
                BlabberKeys.DictA,
                ("foo", "foo"),
                ("bob-fee", "bob-fee"))
            .WithSeedAccounts(
                new Blabber.Emulator.Options.BlabberEmulatorAccount
                {
                    Account = "foo",
                    Bar = "bob-for",
                    Baz = "bob-foz",
                },
                new Blabber.Emulator.Options.BlabberEmulatorAccount
                {
                    Account = "bob-fee",
                    Bar = "bob-fee-bar",
                    Baz = "bob-fee-baz",
                });

        bobStack["primary-api"].WithEmulation(stackOperations, bobBlabberEmulation);

        var stacks = new Dictionary<string, StackResources>(StringComparer.OrdinalIgnoreCase)
        {
            [BoxStackName] = stackOperations.OrchestrateStack(boxStack),
            [BobStackName] = stackOperations.OrchestrateStack(bobStack),
        };

        var bleeb = BleebOrchestrator.OrchestrateApi(stackOperations);
        BleebOrchestrator.WireToPrimaryApis(stackOperations, bleeb, stacks);

        EmulationOrchestrationExtensions.WireOrchestratedEmulationResource(
            stackOperations,
            stacks,
            BleebEmulatorOrchestrationConfiguration.ResourceName,
            BleebOrchestrationConfiguration.PrimaryApiLogicalName,
            BleebEmulatorOrchestrationConfiguration.EmulatorBaseUriEnvironmentVariable,
            BobStackName);

        return stacks;
    }
}
