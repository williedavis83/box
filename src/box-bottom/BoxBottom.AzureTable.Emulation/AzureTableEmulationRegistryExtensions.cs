using BoxBottom.Azure.Table;
using BoxBottom.AzureTable.Emulation.Configuration;
using BoxBottom.Emulation;
using BoxBottom.Emulation.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.AzureTable.Emulation;

public static class AzureTableEmulationRegistryExtensions
{
    public const string AzureTableEmulationKey = "AzureTable_Emulation";

    private static readonly AzureTableEmulationDocumentApplier DocumentApplier = new();

    public static void RegisterAzureTableEmulation(this IEmulationRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.Register<IAzureTableService, AzureTableEmulatorAnchorConfig>(
            AzureTableEmulationKey,
            new EmulatorServiceFactory<IAzureTableService, AzureTableEmulatorAnchorConfig>(
                AzureTableEmulationFactories.CreateEmulatedAzureTableService),
            DocumentApplier);
    }

    public static IEmulationBuilder RegisterAzureTableEmulation(this IEmulationBuilder emulationBuilder)
    {
        ArgumentNullException.ThrowIfNull(emulationBuilder);

        if (emulationBuilder.HostBuilder.Environment.IsProduction())
        {
            return emulationBuilder;
        }

        emulationBuilder.AddRegistryConfigurator(new AzureTableEmulationRegistryConfigurator());
        return emulationBuilder;
    }

    private sealed class AzureTableEmulationRegistryConfigurator : IEmulationRegistryConfigurator
    {
        public void Configure(IEmulationRegistry registry) => registry.RegisterAzureTableEmulation();
    }
}
