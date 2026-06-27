using System.Text.Json;
using BoxBottom.Azure.Table;
using BoxBottom.AzureTable.Emulation.Configuration;
using BoxBottom.Emulation;
using BoxBottom.Emulation.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.AzureTable.Emulation;

public sealed class AzureTableEmulationDocumentApplier : IEmulationRegistryEntryApplier
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public void Apply(
        IHostApplicationBuilder builder,
        JsonDocument jsonDocument,
        IEmulationAnchorRepository anchorRepository,
        EmulationRegistryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(jsonDocument);
        ArgumentNullException.ThrowIfNull(anchorRepository);
        ArgumentNullException.ThrowIfNull(entry);

        var document = jsonDocument.Deserialize<EmulationConfigDocument<AzureTableEmulatorAnchorConfig, object?>>(
            SerializerOptions);

        if (document is null)
        {
            throw new InvalidOperationException("The Azure Table emulation JSON document could not be deserialized.");
        }

        var serviceFactory = entry.GetServiceFactory<IAzureTableService, AzureTableEmulatorAnchorConfig>();
        var remapper = new EmulationRemapper<AzureTableEmulatorAnchorConfig>(anchorRepository, serviceFactory);

        var singletonDocument = new EmulationConfigDocument<AzureTableEmulatorAnchorConfig, object?>
        {
            Singletons = new Dictionary<string, AzureTableEmulatorAnchorConfig>(document.Singletons, StringComparer.OrdinalIgnoreCase),
        };

        using var singletonJson = JsonDocument.Parse(JsonSerializer.Serialize(singletonDocument, SerializerOptions));
        remapper.RemapInjection(builder.Services, singletonJson);
        ApplyDictionaryCompositions(builder.Services, anchorRepository, document);
    }

    private static void ApplyDictionaryCompositions(
        IServiceCollection services,
        IEmulationAnchorRepository repository,
        EmulationConfigDocument<AzureTableEmulatorAnchorConfig, object?> document)
    {
        var anchorsByName = repository.GetAll()
            .ToDictionary(anchor => anchor.AnchorName, StringComparer.OrdinalIgnoreCase);

        foreach (var (anchorName, entries) in document.Dictionaries)
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
                (serviceProvider, _) => entries.ToDictionary(
                    entry => entry.Key,
                    entry => ResolveDictionaryMember(entry.Value, serviceProvider),
                    StringComparer.OrdinalIgnoreCase));

            repository.MarkActivated(anchorName);
        }
    }

    private static IAzureTableService ResolveDictionaryMember(
        AzureTableEmulatorAnchorConfig config,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        if (string.IsNullOrWhiteSpace(config.MemberAnchorKey))
        {
            throw new InvalidOperationException(
                "Azure Table dictionary emulation entries must specify a member anchor key.");
        }

        return serviceProvider.GetRequiredKeyedService<IAzureTableService>(config.MemberAnchorKey);
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
