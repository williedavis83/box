using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BoxBottom.Emulation.Shared;

namespace BoxBottom.Aspire.Orchestration;

public sealed class EmulationOrchestrationState
{
    internal Dictionary<string, EmulationOrchestratedResource> Resources { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, IResourceBuilder<ProjectResource>> ResourceBuilders { get; } =
        new(StringComparer.OrdinalIgnoreCase);
}
