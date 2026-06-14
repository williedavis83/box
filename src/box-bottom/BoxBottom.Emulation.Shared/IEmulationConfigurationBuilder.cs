namespace BoxBottom.Emulation.Shared;

public interface IEmulationConfigurationBuilder
{
    string EmulationKey { get; }

    void OrchestrateEmulationResources(IEmulationResourceOrchestrator orchestrator);
}

public interface IEmulationConfigurationBuilder<TEmulatorConfig, THostedServiceConfig>
    : IEmulationConfigurationBuilder
{
    EmulationConfigDocument<TEmulatorConfig, THostedServiceConfig> BuildDocument();
}
