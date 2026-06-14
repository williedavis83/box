using Blabber.Emulator.Configuration;
using Blabber.Emulator.Services;
using Blabber.Lib;
using BoxBottom.Emulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blabber.Emulator;

public static class BlabberEmulationRegistryExtensions
{
    public const string BlabberEmulationKey = "Blabber_Emulation";

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
                BlabberEmulationFactories.CreateSeedHostedService));
    }

    public static IHostApplicationBuilder RegisterBlabberEmulation(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Environment.IsProduction())
        {
            return builder;
        }

        builder.Services.AddSingleton<IEmulationRegistryConfigurator, BlabberEmulationRegistryConfigurator>();
        builder.Services.AddSingleton<IEmulationConfigurationApplier, BlabberEmulationConfigurationApplier>();
        return builder;
    }

    private sealed class BlabberEmulationRegistryConfigurator : IEmulationRegistryConfigurator
    {
        public void Configure(IEmulationRegistry registry) => registry.RegisterBlabberEmulation();
    }
}
