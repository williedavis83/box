using System.Text.Json;
using BoxBottom.Emulation.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public sealed class EmulationRemapper<TEmulatorConfig, THostedService, THostedServiceConfig> : IEmulationRemapper
    where THostedService : class, IHostedService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IEmulationAnchorRepository _anchorRepository;
    private readonly IEmulatorServiceFactory<TEmulatorConfig> _serviceFactory;
    private readonly EmulatorHostedServiceFactory<THostedService, THostedServiceConfig>? _hostedServiceFactory;

    public EmulationRemapper(
        IEmulationAnchorRepository anchorRepository,
        IEmulatorServiceFactory<TEmulatorConfig> serviceFactory)
    {
        ArgumentNullException.ThrowIfNull(anchorRepository);
        ArgumentNullException.ThrowIfNull(serviceFactory);

        _anchorRepository = anchorRepository;
        _serviceFactory = serviceFactory;
    }

    public EmulationRemapper(
        IEmulationAnchorRepository anchorRepository,
        IEmulatorServiceFactory<TEmulatorConfig> serviceFactory,
        EmulatorHostedServiceFactory<THostedService, THostedServiceConfig> hostedServiceFactory)
    {
        ArgumentNullException.ThrowIfNull(anchorRepository);
        ArgumentNullException.ThrowIfNull(serviceFactory);
        ArgumentNullException.ThrowIfNull(hostedServiceFactory);

        _anchorRepository = anchorRepository;
        _serviceFactory = serviceFactory;
        _hostedServiceFactory = hostedServiceFactory;
    }

    public void RemapInjection(IServiceCollection services, JsonDocument jsonDocument)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(jsonDocument);

        var document = jsonDocument.Deserialize<EmulationConfigDocument<TEmulatorConfig, THostedServiceConfig>>(
            SerializerOptions);

        if (document is null)
        {
            throw new InvalidOperationException("The emulation JSON document could not be deserialized.");
        }

        var anchorsByName = _anchorRepository.GetAll()
            .ToDictionary(anchor => anchor.AnchorName, StringComparer.OrdinalIgnoreCase);

        foreach (var (anchorName, config) in document.Singletons)
        {
            if (!TryGetAnchor(anchorsByName, anchorName, EmulationAnchorKind.Singleton, out var anchor))
            {
                continue;
            }

            EmulationKeyedServiceReplacement.ReplaceKeyed(
                services,
                anchor.ServiceType,
                anchorName,
                anchor.Lifetime,
                (sp, _) => _serviceFactory.CreateSingleton(config, sp));
            _anchorRepository.MarkActivated(anchorName);
        }

        foreach (var (anchorName, configs) in document.Lists)
        {
            if (!TryGetAnchor(anchorsByName, anchorName, EmulationAnchorKind.List, out var anchor))
            {
                continue;
            }

            EmulationKeyedServiceReplacement.ReplaceKeyed(
                services,
                anchor.ServiceType,
                anchorName,
                anchor.Lifetime,
                (sp, _) => _serviceFactory.CreateList(configs, sp));
            _anchorRepository.MarkActivated(anchorName);
        }

        foreach (var (anchorName, configs) in document.Dictionaries)
        {
            if (!TryGetAnchor(anchorsByName, anchorName, EmulationAnchorKind.Dictionary, out var anchor))
            {
                continue;
            }

            EmulationKeyedServiceReplacement.ReplaceKeyed(
                services,
                anchor.ServiceType,
                anchorName,
                anchor.Lifetime,
                (sp, _) => _serviceFactory.CreateDictionary(configs, sp));
            _anchorRepository.MarkActivated(anchorName);
        }

        if (document.HostedServiceConfig is not null)
        {
            if (_hostedServiceFactory is null)
            {
                throw new InvalidOperationException(
                    "The emulation document includes hosted service configuration, but no hosted service factory was provided.");
            }

            services.AddHostedService<THostedService>(sp =>
                _hostedServiceFactory.CreateSingleton(document.HostedServiceConfig, sp));
        }
    }

    private static bool TryGetAnchor(
        IReadOnlyDictionary<string, EmulationAnchorInfo> anchorsByName,
        string anchorName,
        EmulationAnchorKind expectedKind,
        out EmulationAnchorInfo anchor)
    {
        if (!anchorsByName.TryGetValue(anchorName, out anchor!))
        {
            return false;
        }

        if (anchor.Kind != expectedKind)
        {
            return false;
        }

        return true;
    }
}
