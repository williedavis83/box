using Blabber.Emulator.Configuration;
using Blabber.Emulator.Services;
using Blabber.Lib;
using BoxBottom.Emulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blabber.Emulator;

public static class BlabberEmulationRegistryExtensions
{
    public const string BlabberEmulationKey = "Blabber_Emulation";

    private static readonly BlabberEmulationDocumentApplier DocumentApplier = new();

    public static void RegisterBlabberEmulation(this IEmulationRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.Register<
            IBlabber,
            BlabberEmulatorAnchorConfig,
            BleebEmulatorSeedHostedService,
            BlabberEmulatorHostedServiceConfig>(
            BlabberEmulationKey,
            new EmulatorServiceFactory<IBlabber, BlabberEmulatorAnchorConfig>(
                BlabberEmulationFactories.CreateEmulatedBlabber),
            new EmulatorHostedServiceFactory<BleebEmulatorSeedHostedService, BlabberEmulatorHostedServiceConfig>(
                BlabberEmulationFactories.CreateSeedHostedService),
            DocumentApplier);
    }

    public static IEmulationBuilder RegisterBlabberEmulation(this IEmulationBuilder emulationBuilder)
    {
        ArgumentNullException.ThrowIfNull(emulationBuilder);

        if (emulationBuilder.HostBuilder.Environment.IsProduction())
        {
            return emulationBuilder;
        }

        emulationBuilder.AddRegistryConfigurator(new BlabberEmulationRegistryConfigurator());
        return emulationBuilder;
    }

    private sealed class BlabberEmulationRegistryConfigurator : IEmulationRegistryConfigurator
    {
        public void Configure(IEmulationRegistry registry) => registry.RegisterBlabberEmulation();
    }
}
