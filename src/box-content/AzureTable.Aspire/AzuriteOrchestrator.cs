using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using AzureTable.Emulator;
using BoxBottom.Aspire.Orchestration;

namespace AzureTable.Aspire;

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
        params string[] stackNames)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);
        ArgumentNullException.ThrowIfNull(stacks);
        ArgumentException.ThrowIfNullOrWhiteSpace(primaryApiLogicalName);
        ArgumentNullException.ThrowIfNull(stackNames);

        if (_tablesBuilder is null)
        {
            return;
        }

        foreach (var stackName in stackNames)
        {
            if (!stacks.TryGetValue(stackName, out var stackResources)
                || !stackResources.Apis.TryGetValue(primaryApiLogicalName, out var primaryApi))
            {
                continue;
            }

            primaryApi
                .WithReference(_tablesBuilder)
                .WaitFor(_tablesBuilder)
                .WithEnvironment(
                    AzuriteOrchestrationConfiguration.ConnectionStringEnvironmentVariable,
                    _tablesBuilder);
        }
    }

    internal static void ResetForTests() => _tablesBuilder = null;
}
