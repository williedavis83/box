using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BoxBottom.Emulation.Shared;

namespace BoxBottom.Aspire.Orchestration;

public static class EmulationOrchestrationExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly Dictionary<string, EmulationOrchestratedResource> OrchestratedResources =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, IResourceBuilder<ProjectResource>> OrchestratedResourceBuilders =
        new(StringComparer.OrdinalIgnoreCase);

    public static ApiProjectOptions WithEmulation<TEmulatorConfig, THostedServiceConfig>(
        this ApiProjectOptions options,
        StackOperations stackOperations,
        IEmulationConfigurationBuilder<TEmulatorConfig, THostedServiceConfig> builder)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(stackOperations);
        ArgumentNullException.ThrowIfNull(builder);

        var orchestrator = new StackEmulationResourceOrchestrator(
            stackOperations,
            OrchestratedResources,
            OrchestratedResourceBuilders);
        builder.OrchestrateEmulationResources(orchestrator);

        var document = builder.BuildDocument();
        options.EnvironmentVariables[builder.EmulationKey] =
            JsonSerializer.Serialize(document, SerializerOptions);

        return options;
    }

    public static void WireOrchestratedEmulationResource(
        StackOperations stackOperations,
        IReadOnlyDictionary<string, StackResources> stacks,
        string resourceName,
        string primaryApiLogicalName,
        string environmentVariableName,
        params string[] stackNames)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);
        ArgumentNullException.ThrowIfNull(stacks);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(primaryApiLogicalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentVariableName);

        if (!OrchestratedResources.TryGetValue(resourceName, out var orchestratedResource)
            || !OrchestratedResourceBuilders.TryGetValue(resourceName, out var resourceBuilder))
        {
            return;
        }

        foreach (var stackName in stackNames)
        {
            if (!stacks.TryGetValue(stackName, out var stackResources)
                || !stackResources.Apis.TryGetValue(primaryApiLogicalName, out var primaryApi))
            {
                continue;
            }

            primaryApi
                .WithReference(resourceBuilder)
                .WaitFor(resourceBuilder)
                .WithEnvironment(environmentVariableName, orchestratedResource.ServiceAddress);
        }
    }
}
