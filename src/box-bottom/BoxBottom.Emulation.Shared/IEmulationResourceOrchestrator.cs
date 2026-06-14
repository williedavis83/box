namespace BoxBottom.Emulation.Shared;

public interface IEmulationResourceOrchestrator
{
    bool TryGetOrchestratedResource(string resourceName, out EmulationOrchestratedResource resource);

    EmulationOrchestratedResource OrchestrateSupportProject(string resourceName, string projectPath);
}
