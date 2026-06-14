using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BoxBottom.Emulation.Shared;

namespace BoxBottom.Aspire.Orchestration;

public sealed class StackEmulationResourceOrchestrator : IEmulationResourceOrchestrator
{
    private readonly StackOperations _stackOperations;
    private readonly Dictionary<string, EmulationOrchestratedResource> _orchestratedResources;
    private readonly Dictionary<string, IResourceBuilder<ProjectResource>> _orchestratedResourceBuilders;

    public StackEmulationResourceOrchestrator(
        StackOperations stackOperations,
        Dictionary<string, EmulationOrchestratedResource> orchestratedResources,
        Dictionary<string, IResourceBuilder<ProjectResource>> orchestratedResourceBuilders)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);
        ArgumentNullException.ThrowIfNull(orchestratedResources);
        ArgumentNullException.ThrowIfNull(orchestratedResourceBuilders);

        _stackOperations = stackOperations;
        _orchestratedResources = orchestratedResources;
        _orchestratedResourceBuilders = orchestratedResourceBuilders;
    }

    public bool TryGetOrchestratedResource(string resourceName, out EmulationOrchestratedResource resource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        return _orchestratedResources.TryGetValue(resourceName, out resource!);
    }

    public EmulationOrchestratedResource OrchestrateSupportProject(string resourceName, string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        if (_orchestratedResources.TryGetValue(resourceName, out var existing))
        {
            return existing;
        }

        var resourceBuilder = _stackOperations.OrchestrateSupport(builder =>
            builder.AddSupportProject(resourceName, projectPath));

        var orchestratedResource = new EmulationOrchestratedResource(
            resourceName,
            EdgeRoutingConfiguration.BuildApiClusterAddress(resourceName));

        _orchestratedResources[resourceName] = orchestratedResource;
        _orchestratedResourceBuilders[resourceName] = resourceBuilder;
        return orchestratedResource;
    }
}
