using BoxBottom.Azure.Table;
using BoxBottom.AzureTable.Emulation.Configuration;
using BoxBottom.Emulation;

namespace BoxBottom.AzureTable.Emulation;

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
