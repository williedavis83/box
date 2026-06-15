using System.Net.Http.Json;
using Blabber.Emulator;
using BoxBottom.Emulation;
using Foo.Primary.Api.Controllers;
using Foo.Primary.Api.Blabber;
using Foo.Primary.Shared.Blabber;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class EmulationRegistryApplierTests
{
    [Fact]
    public void ApplyConfiguredEmulation_ActivatesAnchorsWhenBlabberDocumentPresent()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Bleeb:BaseUri"] = "http://bleeb.test/",
            ["BlabberEmulator:BleebEmulatorBaseUri"] = "http://bleeb-emulator.test/",
            ["Blabber_Emulation"] =
                """
                {
                  "singletons": {
                    "foo": { "account": "foo" }
                  },
                  "lists": {
                    "list-a": [{ "account": "foo" }]
                  },
                  "dictionaries": {
                    "dict-a": {
                      "foo": { "account": "foo" }
                    }
                  }
                }
                """,
        });

        builder.Services.AddBlabbers(builder.Configuration);
        builder.AddEmulation(typeof(Foo.Primary.Api.Controllers.BlabberController).Assembly)
            .RegisterBlabberEmulation()
            .ApplyConfiguredEmulation();

        using var provider = builder.Services.BuildServiceProvider();
        var repository = provider.GetRequiredService<IEmulationAnchorRepository>();

        Assert.True(repository.IsActivated(BlabberKeys.Foo));
        Assert.True(repository.IsActivated(BlabberKeys.ListA));
        Assert.True(repository.IsActivated(BlabberKeys.DictA));
    }

    [Fact]
    public void ApplyConfiguredEmulation_SkipsWhenBlabberDocumentMissing()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Bleeb:BaseUri"] = "http://bleeb.test/",
        });

        builder.Services.AddBlabbers(builder.Configuration);
        builder.AddEmulation(typeof(Foo.Primary.Api.Controllers.BlabberController).Assembly)
            .RegisterBlabberEmulation()
            .ApplyConfiguredEmulation();

        using var provider = builder.Services.BuildServiceProvider();
        var repository = provider.GetRequiredService<IEmulationAnchorRepository>();

        Assert.False(repository.IsActivated(BlabberKeys.Foo));
        Assert.False(repository.IsActivated(BlabberKeys.ListA));
        Assert.False(repository.IsActivated(BlabberKeys.DictA));
    }
}

public class EmulationStartupTests
{
    [Fact]
    public async Task GetAnchors_WithoutEmulation_AllRegisteredNoneActivated()
    {
        await using var factory = new EmulationWebApplicationFactory(includeEmulation: false);
        using var client = factory.CreateClient();

        var anchors = await client.GetFromJsonAsync<List<EmulationAnchorDiagnostic>>("/api/emulation/anchors");

        Assert.Equal(3, anchors!.Count);
        Assert.All(anchors, anchor => Assert.False(anchor.IsActivated));
    }

    [Fact]
    public async Task GetAnchors_WithBobEmulation_ActivatesConfiguredAnchors()
    {
        await using var factory = new EmulationWebApplicationFactory(includeEmulation: true);
        using var client = factory.CreateClient();

        var anchors = await client.GetFromJsonAsync<List<EmulationAnchorDiagnostic>>("/api/emulation/anchors");

        Assert.Equal(3, anchors!.Count);
        Assert.True(anchors.Single(anchor => anchor.AnchorName == BlabberKeys.Foo).IsActivated);
        Assert.True(anchors.Single(anchor => anchor.AnchorName == BlabberKeys.ListA).IsActivated);
        Assert.True(anchors.Single(anchor => anchor.AnchorName == BlabberKeys.DictA).IsActivated);
    }

    private sealed record EmulationAnchorDiagnostic(
        string AssemblyName,
        string AnchorName,
        string ServiceType,
        string Kind,
        string Lifetime,
        bool IsActivated);

    private sealed class EmulationWebApplicationFactory : WebApplicationFactory<BlabberController>
    {
        private readonly bool _includeEmulation;

        public EmulationWebApplicationFactory(bool includeEmulation) =>
            _includeEmulation = includeEmulation;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Development);
            builder.UseSetting("Bleeb:BaseUri", "http://bleeb.test/");

            if (_includeEmulation)
            {
                builder.UseSetting("BlabberEmulator:BleebEmulatorBaseUri", "http://bleeb-emulator.test/");
                builder.UseSetting("Blabber_Emulation", BobEmulationJson);
            }
        }

        private const string BobEmulationJson =
            """
            {
              "singletons": {
                "foo": { "account": "foo" }
              },
              "lists": {
                "list-a": [{ "account": "foo" }, { "account": "bob-fee" }]
              },
              "dictionaries": {
                "dict-a": {
                  "foo": { "account": "foo" },
                  "bob-fee": { "account": "bob-fee" }
                }
              }
            }
            """;
    }
}
