using Blabber.Emulator.Options;
using Blabber.Emulator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blabber.Emulator;

public static class BlabberEmulatorServiceCollectionExtensions
{
    public static IServiceCollection AddBlabberEmulator(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<BlabberEmulatorOptions>(
            configuration.GetSection(BlabberEmulatorOptions.SectionName));
        services.AddHttpClient(BleebEmulatorSeedHostedService.HttpClientName);
        services.AddHostedService<BleebEmulatorSeedHostedService>();

        return services;
    }
}
