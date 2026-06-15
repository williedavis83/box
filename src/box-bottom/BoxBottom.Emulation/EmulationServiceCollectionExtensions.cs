using System.Reflection;
using BoxBottom.Emulation.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public static class EmulationServiceCollectionExtensions
{
    public static IEmulationBuilder AddEmulation(
        this IHostApplicationBuilder builder,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Environment.IsProduction())
        {
            return new NoOpEmulationBuilder(builder);
        }

        var repository = new EmulationAnchorRepository();

        foreach (var anchor in EmulationAnchorScanner.Scan(
                     EmulationAnchorScanner.CollectAssemblies(assemblies)))
        {
            repository.Add(anchor);
        }

        var registry = new EmulationRegistry();
        var emulationBuilder = new EmulationBuilder(builder, repository, registry);

        builder.Services.AddSingleton<IEmulationAnchorRepository>(repository);
        builder.Services.AddSingleton<IEmulationRegistry>(registry);

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(EmulationDiagnosticController).Assembly);

        return emulationBuilder;
    }

    [Obsolete("Use AddEmulation(...).ApplyConfiguredEmulation() instead.")]
    public static IHostApplicationBuilder EnableEmulationSupport(
        this IHostApplicationBuilder builder,
        params Assembly[] assemblies)
    {
        builder.AddEmulation(assemblies);
        return builder;
    }

    [Obsolete("Use AddEmulation(...).ApplyConfiguredEmulation() instead.")]
    public static IHostApplicationBuilder ApplyConfiguredEmulation(this IHostApplicationBuilder builder) =>
        builder;

    private sealed class NoOpEmulationBuilder(IHostApplicationBuilder hostBuilder) : IEmulationBuilder
    {
        public IHostApplicationBuilder HostBuilder { get; } = hostBuilder;

        public IEmulationAnchorRepository AnchorRepository { get; } = new EmulationAnchorRepository();

        public IEmulationRegistry Registry { get; } = new EmulationRegistry();

        public IEmulationBuilder ApplyConfiguredEmulation() => this;

        public IEmulationBuilder AddRegistryConfigurator(IEmulationRegistryConfigurator configurator) => this;
    }
}
