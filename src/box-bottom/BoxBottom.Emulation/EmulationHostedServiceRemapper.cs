using System.Text.Json;
using BoxBottom.Emulation.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public sealed class EmulationHostedServiceRemapper<TEmulatorConfig, THostedService, THostedServiceConfig> : IEmulationRemapper
    where THostedService : class, IHostedService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly EmulationRemapper<TEmulatorConfig> _remapper;
    private readonly EmulatorHostedServiceFactory<THostedService, THostedServiceConfig> _hostedServiceFactory;

    public EmulationHostedServiceRemapper(
        IEmulationAnchorRepository anchorRepository,
        IEmulatorServiceFactory<TEmulatorConfig> serviceFactory,
        EmulatorHostedServiceFactory<THostedService, THostedServiceConfig> hostedServiceFactory)
    {
        ArgumentNullException.ThrowIfNull(anchorRepository);
        ArgumentNullException.ThrowIfNull(serviceFactory);
        ArgumentNullException.ThrowIfNull(hostedServiceFactory);

        _remapper = new EmulationRemapper<TEmulatorConfig>(anchorRepository, serviceFactory);
        _hostedServiceFactory = hostedServiceFactory;
    }

    public void RemapInjection(IServiceCollection services, JsonDocument jsonDocument)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(jsonDocument);

        _remapper.RemapInjection(services, jsonDocument);

        var document = jsonDocument.Deserialize<EmulationConfigDocument<TEmulatorConfig, THostedServiceConfig>>(
            SerializerOptions);

        if (document is null || document.HostedServiceConfig is null)
        {
            return;
        }

        var hostedServiceConfig = document.HostedServiceConfig;
        services.AddHostedService<THostedService>(sp =>
            _hostedServiceFactory.CreateSingleton(hostedServiceConfig, sp));
    }
}
