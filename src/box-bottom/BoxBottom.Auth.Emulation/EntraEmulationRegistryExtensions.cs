using BoxBottom.Auth.Emulation.Configuration;
using BoxBottom.Emulation;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Auth.Emulation;

public static class EntraEmulationRegistryExtensions
{
    public const string EntraEmulationKey = "Auth_Emulation";
    public const string EntraAnchorName = "Entra";

    private static readonly EntraEmulationDocumentApplier DocumentApplier = new();

    public static void RegisterEntraEmulation(this IEmulationRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.Register<EntraEmulatorAnchorConfig, EntraEmulatorAnchorConfig>(
            EntraEmulationKey,
            new EmulatorServiceFactory<EntraEmulatorAnchorConfig, EntraEmulatorAnchorConfig>(
                (config, _) => config),
            DocumentApplier);
    }

    public static IEmulationBuilder RegisterEntraEmulation(this IEmulationBuilder emulationBuilder)
    {
        ArgumentNullException.ThrowIfNull(emulationBuilder);

        if (emulationBuilder.HostBuilder.Environment.IsProduction())
        {
            return emulationBuilder;
        }

        emulationBuilder.AddRegistryConfigurator(new EntraEmulationRegistryConfigurator());
        return emulationBuilder;
    }

    private sealed class EntraEmulationRegistryConfigurator : IEmulationRegistryConfigurator
    {
        public void Configure(IEmulationRegistry registry) => registry.RegisterEntraEmulation();
    }
}
