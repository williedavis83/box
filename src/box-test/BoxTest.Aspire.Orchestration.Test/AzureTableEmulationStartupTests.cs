using BoxBottom.AzureTable.Emulation;
using BoxBottom.Azure.Table;
using BoxBottom.Emulation;
using Foo.Primary.Api.AzureTable;
using Foo.Primary.Api.Controllers;
using Foo.Primary.Shared.AzureTable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AzureTableEmulationStartupTests
{
    [Fact]
    public void ApplyConfiguredEmulation_ActivatesSingletonAndDictionaryAnchors()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AzureTableAccounts:table-orders:ServiceUri"] = "https://example.table.core.windows.net/",
            ["AzureTableAccounts:table-analytics:ServiceUri"] = "https://example.table.core.windows.net/",
            ["AzureTableAccounts:table-geo-us-east:ServiceUri"] = "https://example.table.core.windows.net/",
            ["AzureTableAccounts:table-geo-eu-west:ServiceUri"] = "https://example.table.core.windows.net/",
            ["AzureTableConnectionString:ConnectionString"] = "UseDevelopmentStorage=true",
            ["AzureTable_Emulation"] =
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
                """,
        });

        builder.Services.AddFooAzureTableScenarios(builder.Configuration);
        builder.AddEmulation(typeof(AzureTableBasicsController).Assembly)
            .RegisterAzureTableEmulation()
            .ApplyConfiguredEmulation();

        using var provider = builder.Services.BuildServiceProvider();
        var repository = provider.GetRequiredService<IEmulationAnchorRepository>();
        var orders = provider.GetRequiredKeyedService<IAzureTableService>(AzureTableKeys.Orders);
        var replicas = provider.GetRequiredKeyedService<IReadOnlyDictionary<string, IAzureTableService>>(AzureTableKeys.GeoReplicas);

        Assert.True(repository.IsActivated(AzureTableKeys.Orders));
        Assert.True(repository.IsActivated(AzureTableKeys.Analytics));
        Assert.True(repository.IsActivated(AzureTableKeys.GeoUsEast));
        Assert.True(repository.IsActivated(AzureTableKeys.GeoEuWest));
        Assert.True(repository.IsActivated(AzureTableKeys.GeoReplicas));
        Assert.Equal("ConnectionString", orders.GetConfigurationStatus().Mode);
        Assert.Equal("ConnectionString", replicas[AzureTableGeoKeys.UsEast].GetConfigurationStatus().Mode);
        Assert.Equal("ConnectionString", replicas[AzureTableGeoKeys.EuWest].GetConfigurationStatus().Mode);
    }

    [Fact]
    public void ApplyConfiguredEmulation_SkipsWhenDocumentMissing()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AzureTableAccounts:table-orders:ServiceUri"] = "https://example.table.core.windows.net/",
        });

        builder.Services.AddFooAzureTableScenarios(builder.Configuration);
        builder.AddEmulation(typeof(AzureTableBasicsController).Assembly)
            .RegisterAzureTableEmulation()
            .ApplyConfiguredEmulation();

        using var provider = builder.Services.BuildServiceProvider();
        var repository = provider.GetRequiredService<IEmulationAnchorRepository>();
        var orders = provider.GetRequiredKeyedService<IAzureTableService>(AzureTableKeys.Orders);

        Assert.False(repository.IsActivated(AzureTableKeys.Orders));
        Assert.Equal("Uri", orders.GetConfigurationStatus().Mode);
    }
}
