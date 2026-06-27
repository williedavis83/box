using BoxBottom.Aspire.Orchestration;
using BoxBottom.Auth.Emulation;

namespace BoxBottom.Auth.Aspire;

public static class EntraEmulationOrchestrationExtensions
{
    public static ApiProjectOptions WithEntraEmulation(
        this ApiProjectOptions options,
        StackOperations stackOperations,
        Action<EntraEmulationConfigurationBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(stackOperations);

        var builder = new EntraEmulationConfigurationBuilder().OverrideEntra();
        configure?.Invoke(builder);

        KeycloakOrchestrator.Orchestrate(stackOperations);

        return options.WithEmulation(stackOperations, builder);
    }
}
