using System.Net.Http.Json;
using Blabber.Emulator;
using BoxBottom.Emulation;
using Foo.Primary.Api.Controllers;
using Foo.Primary.Api.Blabber;
using BoxBottom.Azure.Table;
using BoxBottom.Azure.Table.Services;
using Foo.Primary.Shared.AzureTable;
using Foo.Primary.Shared.Blabber;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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

        Assert.Equal(8, anchors!.Count);
        Assert.All(
            anchors.Where(anchor =>
                anchor.AnchorName is not AzureTableKeys.Orders
                and not AzureTableKeys.Analytics
                and not AzureTableKeys.GeoUsEast
                and not AzureTableKeys.GeoEuWest
                and not AzureTableKeys.GeoReplicas),
            anchor => Assert.False(anchor.IsActivated));
    }

    [Fact]
    public async Task GetAnchors_WithBobEmulation_ActivatesConfiguredAnchors()
    {
        await using var factory = new EmulationWebApplicationFactory(includeEmulation: true);
        using var client = factory.CreateClient();

        var anchors = await client.GetFromJsonAsync<List<EmulationAnchorDiagnostic>>("/api/emulation/anchors");

        Assert.Equal(8, anchors!.Count);
        Assert.True(anchors.Single(anchor => anchor.AnchorName == BlabberKeys.Foo).IsActivated);
        Assert.True(anchors.Single(anchor => anchor.AnchorName == AzureTableKeys.Orders).IsActivated);
        Assert.True(anchors.Single(anchor => anchor.AnchorName == AzureTableKeys.GeoReplicas).IsActivated);
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
            builder.ConfigureTestServices(services =>
            {
                var bootstrapDescriptors = services
                    .Where(descriptor => descriptor.ImplementationType == typeof(AzureTableBootstrapHostedService))
                    .ToList();

                foreach (var descriptor in bootstrapDescriptors)
                {
                    services.Remove(descriptor);
                }
            });

            builder.UseEnvironment(Environments.Development);
            builder.UseSetting("Bleeb:BaseUri", "http://bleeb.test/");
            builder.UseSetting("AzureTableAccounts:table-orders:ServiceUri", "https://example.table.core.windows.net/");
            builder.UseSetting("AzureTableAccounts:table-analytics:ServiceUri", "https://example.table.core.windows.net/");
            builder.UseSetting("AzureTableAccounts:table-geo-us-east:ServiceUri", "https://example.table.core.windows.net/");
            builder.UseSetting("AzureTableAccounts:table-geo-eu-west:ServiceUri", "https://example.table.core.windows.net/");

            if (_includeEmulation)
            {
                builder.UseSetting("BlabberEmulator:BleebEmulatorBaseUri", "http://bleeb-emulator.test/");
                builder.UseSetting("AzureTableConnectionString:ConnectionString", "UseDevelopmentStorage=true");
                builder.UseSetting("Blabber_Emulation", BobBlabberEmulationJson);
                builder.UseSetting("AzureTable_Emulation", BobAzureTableEmulationJson);
            }
        }

        private const string BobBlabberEmulationJson =
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

        private const string BobAzureTableEmulationJson =
            """
            {
              "singletons": {
                "table-orders": {},
                "table-analytics": {},
                "table-geo-us-east": {},
                "table-geo-eu-west": {}
              },
              "dictionaries": {
                "table-geo-replicas": {
                  "us-east": { "memberAnchorKey": "table-geo-us-east" },
                  "eu-west": { "memberAnchorKey": "table-geo-eu-west" }
                }
              }
            }
            """;
    }
}
