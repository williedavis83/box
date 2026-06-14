using System.Text.Json;
using Blabber.Emulator.Configuration;
using Blabber.Emulator.Options;
using Blabber.Emulator.Services;
using Blabber.Lib;
using BoxBottom.Emulation;
using BoxBottom.Emulation.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blabber.Emulator;

public sealed class BlabberEmulationConfigurationApplier : IEmulationConfigurationApplier
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public string EmulationKey => BlabberEmulationRegistryExtensions.BlabberEmulationKey;

    public void Apply(
        IHostApplicationBuilder builder,
        JsonDocument jsonDocument,
        IEmulationAnchorRepository anchorRepository)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(jsonDocument);
        ArgumentNullException.ThrowIfNull(anchorRepository);

        var document = jsonDocument.Deserialize<EmulationConfigDocument<BlabberEmulatorAnchorConfig, BlabberEmulatorHostedServiceConfig>>(
            SerializerOptions);

        if (document is null)
        {
            throw new InvalidOperationException("The Blabber emulation JSON document could not be deserialized.");
        }

        builder.Services.AddHttpClient(BleebEmulatorSeedHostedService.HttpClientName);

        var singletonDocument = new EmulationConfigDocument<BlabberEmulatorAnchorConfig, BlabberEmulatorHostedServiceConfig>
        {
            Singletons = new Dictionary<string, BlabberEmulatorAnchorConfig>(document.Singletons, StringComparer.OrdinalIgnoreCase),
            HostedServiceConfig = document.HostedServiceConfig,
        };

        using var singletonJson = JsonDocument.Parse(JsonSerializer.Serialize(singletonDocument, SerializerOptions));

        var serviceFactory = new EmulatorServiceFactory<IBlabber, BlabberEmulatorAnchorConfig>(
            BlabberEmulationFactories.CreateEmulatedBlabber);
        var hostedServiceFactory = new EmulatorHostedServiceFactory<
            BleebEmulatorSeedHostedService,
            BlabberEmulatorHostedServiceConfig>(BlabberEmulationFactories.CreateSeedHostedService);

        var remapper = new EmulationRemapper<
            BlabberEmulatorAnchorConfig,
            BleebEmulatorSeedHostedService,
            BlabberEmulatorHostedServiceConfig>(
            anchorRepository,
            serviceFactory,
            hostedServiceFactory);

        remapper.RemapInjection(builder.Services, singletonJson);

        ApplyNamedBlabberCompositions(builder.Services, anchorRepository, document);
    }

    private static void ApplyNamedBlabberCompositions(
        IServiceCollection services,
        IEmulationAnchorRepository repository,
        EmulationConfigDocument<BlabberEmulatorAnchorConfig, BlabberEmulatorHostedServiceConfig> document)
    {
        var anchorsByName = repository.GetAll()
            .ToDictionary(anchor => anchor.AnchorName, StringComparer.OrdinalIgnoreCase);

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
                (sp, _) => configs
                    .Select(config => new NamedBlabber(
                        config.Account,
                        BlabberEmulationFactories.CreateEmulatedBlabber(config, sp)))
                    .ToList());
            repository.MarkActivated(anchorName);
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
                (sp, _) => configs.ToDictionary(
                    entry => entry.Key,
                    entry => BlabberEmulationFactories.CreateEmulatedBlabber(entry.Value, sp),
                    StringComparer.OrdinalIgnoreCase));
            repository.MarkActivated(anchorName);
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

        return anchor.Kind == expectedKind;
    }
}

internal static class BlabberEmulationFactories
{
    public static IBlabber CreateEmulatedBlabber(BlabberEmulatorAnchorConfig config, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var emulatorBaseUri = serviceProvider
            .GetRequiredService<IConfiguration>()
            .GetSection(BlabberEmulatorOptions.SectionName)
            .Get<BlabberEmulatorOptions>()
            ?.BleebEmulatorBaseUri;

        if (emulatorBaseUri is null)
        {
            throw new InvalidOperationException(
                $"Bleeb emulator base URI is not configured. Set '{BlabberEmulatorOptions.SectionName}:BleebEmulatorBaseUri'.");
        }

        var httpClient = serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(BleebEmulatorSeedHostedService.HttpClientName);

        return new FakeBlabber(new global::Blabber.Lib.Blabber(config.Account, emulatorBaseUri, httpClient));
    }

    public static BleebEmulatorSeedHostedService CreateSeedHostedService(
        BlabberEmulatorHostedServiceConfig config,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        return new BleebEmulatorSeedHostedService(
            Microsoft.Extensions.Options.Options.Create(config.Options),
            serviceProvider.GetRequiredService<IHttpClientFactory>(),
            serviceProvider.GetRequiredService<ILogger<BleebEmulatorSeedHostedService>>());
    }
}
