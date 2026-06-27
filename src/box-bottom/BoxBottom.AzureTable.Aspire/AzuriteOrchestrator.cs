using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using BoxBottom.Aspire.Orchestration;
using BoxBottom.AzureTable.Emulation;

namespace BoxBottom.AzureTable.Aspire;

public static class AzuriteOrchestrator
{
    private static IResourceBuilder<AzureTableStorageResource>? _tablesBuilder;

    public static void Orchestrate(StackOperations stackOperations)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);

        if (_tablesBuilder is not null)
        {
            return;
        }

        _tablesBuilder = stackOperations.OrchestrateSupport(builder =>
        {
            var storage = builder
                .AddAzureStorage(AzuriteOrchestrationConfiguration.ResourceName)
                .RunAsEmulator();

            return storage.AddTables(AzuriteOrchestrationConfiguration.TablesResourceName);
        });
    }

    public static void WireToPrimaryApis(
        StackOperations stackOperations,
        IReadOnlyDictionary<string, StackResources> stacks,
        string primaryApiLogicalName,
        params string[] stackNames) =>
        WireToApis(stackOperations, stacks, stackNames, primaryApiLogicalName);

    public static void WireToApis(
        StackOperations stackOperations,
        IReadOnlyDictionary<string, StackResources> stacks,
        string[] stackNames,
        params string[] apiLogicalNames)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);
        ArgumentNullException.ThrowIfNull(stacks);
        ArgumentNullException.ThrowIfNull(stackNames);
        ArgumentNullException.ThrowIfNull(apiLogicalNames);

        if (_tablesBuilder is null)
        {
            return;
        }

        foreach (var stackName in stackNames)
        {
            if (!stacks.TryGetValue(stackName, out var stackResources))
            {
                continue;
            }

            foreach (var apiLogicalName in apiLogicalNames)
            {
                if (!stackResources.Apis.TryGetValue(apiLogicalName, out var api))
                {
                    continue;
                }

                api
                    .WithReference(_tablesBuilder)
                    .WaitFor(_tablesBuilder)
                    .WithEnvironment(
                        AzuriteOrchestrationConfiguration.ConnectionStringEnvironmentVariable,
                        _tablesBuilder);
            }
        }
    }

    internal static void ResetForTests() => _tablesBuilder = null;
}
