using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BoxBottom.Aspire.Orchestration;

namespace Bleeb.Aspire;

public static class BleebOrchestrator
{
    public static IResourceBuilder<ProjectResource> OrchestrateApi(StackOperations stackOperations)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);

        return stackOperations.OrchestrateSupport(builder =>
            builder.AddSupportProject(
                BleebOrchestrationConfiguration.ResourceName,
                BleebOrchestrationConfiguration.ApiProjectPath));
    }

    public static void WireToPrimaryApis(
        StackOperations stackOperations,
        IResourceBuilder<ProjectResource> bleeb,
        IReadOnlyDictionary<string, StackResources> stacks)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);
        ArgumentNullException.ThrowIfNull(bleeb);
        ArgumentNullException.ThrowIfNull(stacks);

        var bleebServiceAddress = BleebOrchestrationConfiguration.BuildBleebServiceAddress(
            bleeb.Resource.Name);

        foreach (var stack in stacks.Values)
        {
            if (!stack.Apis.TryGetValue(
                    BleebOrchestrationConfiguration.PrimaryApiLogicalName,
                    out var primaryApi))
            {
                continue;
            }

            primaryApi
                .WithReference(bleeb)
                .WaitFor(bleeb)
                .WithEnvironment(
                    BleebOrchestrationConfiguration.BaseUriEnvironmentVariable,
                    bleebServiceAddress);
        }
    }
}
