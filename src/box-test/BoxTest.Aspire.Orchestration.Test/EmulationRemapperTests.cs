using System.Text.Json;
using BoxBottom.Emulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class EmulationRemapperTests
{
    [Fact]
    public void RemapInjection_ReplacesMatchedSingletonAnchor()
    {
        const string anchorName = "WidgetA";
        var repository = CreateRepository(anchorName, EmulationAnchorLifetime.Singleton);

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IWidget>(anchorName, new Widget("real"));

        var remapper = CreateRemapper(repository);

        using var json = JsonDocument.Parse(
            """
            {
              "Singletons": {
                "WidgetA": { "Label": "emu" }
              }
            }
            """);

        remapper.RemapInjection(services, json);

        var provider = services.BuildServiceProvider();
        var widget = provider.GetRequiredKeyedService<IWidget>(anchorName);

        Assert.Equal("fake:emu", widget.Label);
        Assert.True(repository.IsActivated(anchorName));
    }

    [Fact]
    public void RemapInjection_DoesNotMarkMissingAnchorsAsActivated()
    {
        var repository = new EmulationAnchorRepository();

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IWidget>("WidgetA", new Widget("real"));

        var remapper = CreateRemapper(repository);

        using var json = JsonDocument.Parse(
            """
            {
              "Singletons": {
                "WidgetA": { "Label": "emu" }
              }
            }
            """);

        remapper.RemapInjection(services, json);

        Assert.False(repository.IsActivated("WidgetA"));
    }

    [Fact]
    public void RemapInjection_SkipsAnchorsMissingFromRepository()
    {
        var repository = new EmulationAnchorRepository();

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IWidget>("WidgetA", new Widget("real"));

        var remapper = CreateRemapper(repository);

        using var json = JsonDocument.Parse(
            """
            {
              "Singletons": {
                "WidgetA": { "Label": "emu" }
              }
            }
            """);

        remapper.RemapInjection(services, json);

        var provider = services.BuildServiceProvider();
        var widget = provider.GetRequiredKeyedService<IWidget>("WidgetA");

        Assert.Equal("real", widget.Label);
    }

    [Fact]
    public void RemapInjection_Transient_ResolvesNewInstanceEachTime()
    {
        const string anchorName = "WidgetA";
        var repository = CreateRepository(anchorName, EmulationAnchorLifetime.Transient);

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IWidget>(anchorName, new Widget("real"));

        var remapper = CreateRemapper(repository);

        using var json = JsonDocument.Parse(
            """
            {
              "Singletons": {
                "WidgetA": { "Label": "emu" }
              }
            }
            """);

        remapper.RemapInjection(services, json);

        var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredKeyedService<IWidget>(anchorName);
        var second = provider.GetRequiredKeyedService<IWidget>(anchorName);

        Assert.Equal("fake:emu", first.Label);
        Assert.Equal("fake:emu", second.Label);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void RemapInjection_Scoped_ResolvesSameInstanceWithinScope()
    {
        const string anchorName = "WidgetA";
        var repository = CreateRepository(anchorName, EmulationAnchorLifetime.Scoped);

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IWidget>(anchorName, new Widget("real"));

        var remapper = CreateRemapper(repository);

        using var json = JsonDocument.Parse(
            """
            {
              "Singletons": {
                "WidgetA": { "Label": "emu" }
              }
            }
            """);

        remapper.RemapInjection(services, json);

        var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        var scope1First = scope1.ServiceProvider.GetRequiredKeyedService<IWidget>(anchorName);
        var scope1Second = scope1.ServiceProvider.GetRequiredKeyedService<IWidget>(anchorName);

        using var scope2 = provider.CreateScope();
        var scope2Instance = scope2.ServiceProvider.GetRequiredKeyedService<IWidget>(anchorName);

        Assert.Same(scope1First, scope1Second);
        Assert.NotSame(scope1First, scope2Instance);
    }

    [Fact]
    public void RemapInjection_RegistersHostedServiceWithStrongType()
    {
        var repository = new EmulationAnchorRepository();

        var services = new ServiceCollection();
        var remapper = new EmulationRemapper<WidgetConfig, TestHostedService, HostedServiceConfig>(
            repository,
            new EmulatorServiceFactory<IWidget, WidgetConfig>((config, _) => new Widget($"fake:{config.Label}")),
            new EmulatorHostedServiceFactory<TestHostedService, HostedServiceConfig>(
                (config, _) => new TestHostedService(config.Label)));

        using var json = JsonDocument.Parse(
            """
            {
              "HostedServiceConfig": { "Label": "seed" }
            }
            """);

        remapper.RemapInjection(services, json);

        var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>().OfType<TestHostedService>().ToList();

        Assert.Single(hostedServices);
        Assert.Equal("seed", hostedServices[0].Label);
    }

    private static EmulationRemapper<WidgetConfig, NoOpHostedService, object> CreateRemapper(
        EmulationAnchorRepository repository) =>
        new(
            repository,
            new EmulatorServiceFactory<IWidget, WidgetConfig>((config, _) => new Widget($"fake:{config.Label}")));

    private static EmulationAnchorRepository CreateRepository(
        string anchorName,
        EmulationAnchorLifetime lifetime)
    {
        var repository = new EmulationAnchorRepository();
        repository.Add(new EmulationAnchorInfo(
            typeof(EmulationRemapperTests).Assembly.GetName().Name!,
            anchorName,
            typeof(IWidget),
            EmulationAnchorKind.Singleton,
            lifetime));
        return repository;
    }

    private interface IWidget
    {
        string Label { get; }
    }

    private sealed class Widget(string label) : IWidget
    {
        public string Label { get; } = label;
    }

    private sealed record WidgetConfig(string Label);

    private sealed record HostedServiceConfig(string Label);

    private sealed class NoOpHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class TestHostedService(string label) : IHostedService
    {
        public string Label { get; } = label;

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
