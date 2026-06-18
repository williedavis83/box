using AzureTable.Emulator.Configuration;
using BoxBottom.Azure.Table;
using BoxBottom.Emulation;

namespace AzureTable.Emulator;

internal static class AzureTableEmulationFactories
{
    public static IAzureTableService CreateEmulatedAzureTableService(
        AzureTableEmulatorAnchorConfig config,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        return AzureTableServiceFactories.CreateConnectionStringService(serviceProvider);
    }
}
