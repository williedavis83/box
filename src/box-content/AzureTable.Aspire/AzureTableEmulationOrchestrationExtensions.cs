using AzureTable.Emulator;
using BoxBottom.Aspire.Orchestration;

namespace AzureTable.Aspire;

public static class AzureTableEmulationOrchestrationExtensions
{
    public static ApiProjectOptions WithAzureTableEmulation(
        this ApiProjectOptions options,
        StackOperations stackOperations,
        Action<AzureTableEmulationConfigurationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(stackOperations);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new AzureTableEmulationConfigurationBuilder();
        configure(builder);

        AzuriteOrchestrator.Orchestrate(stackOperations);

        return options.WithEmulation(stackOperations, builder);
    }
}
