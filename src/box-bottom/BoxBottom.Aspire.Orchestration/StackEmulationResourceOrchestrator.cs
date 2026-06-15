using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BoxBottom.Emulation.Shared;

namespace BoxBottom.Aspire.Orchestration;

public sealed class StackEmulationResourceOrchestrator : IEmulationResourceOrchestrator
{
    private readonly StackOperations _stackOperations;
    private readonly EmulationOrchestrationState _orchestrationState;

    public StackEmulationResourceOrchestrator(
        StackOperations stackOperations,
        EmulationOrchestrationState orchestrationState)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);
        ArgumentNullException.ThrowIfNull(orchestrationState);

        _stackOperations = stackOperations;
        _orchestrationState = orchestrationState;
    }

    public bool TryGetOrchestratedResource(string resourceName, out EmulationOrchestratedResource resource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        return _orchestrationState.Resources.TryGetValue(resourceName, out resource!);
    }

    public EmulationOrchestratedResource OrchestrateSupportProject(string resourceName, string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        if (_orchestrationState.Resources.TryGetValue(resourceName, out var existing))
        {
            return existing;
        }

        var resourceBuilder = _stackOperations.OrchestrateSupport(builder =>
            builder.AddSupportProject(resourceName, projectPath));

        var orchestratedResource = new EmulationOrchestratedResource(
            resourceName,
            EdgeRoutingConfiguration.BuildApiClusterAddress(resourceName));

        _orchestrationState.Resources[resourceName] = orchestratedResource;
        _orchestrationState.ResourceBuilders[resourceName] = resourceBuilder;
        return orchestratedResource;
    }
}
