using AzureTable.Emulator;
using BoxBottom.Azure.Table;
using Foo.Primary.Shared.AzureTable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AzureTableServiceCollectionTests
{
    [Fact]
    public void GetConfigurationStatus_ReturnsNone_WhenAccountUnconfigured()
    {
        var services = CreateServices(new Dictionary<string, string?>(), AzureTableKeys.Orders);
        using var provider = services.BuildServiceProvider();

        var status = provider.GetRequiredKeyedService<IAzureTableService>(AzureTableKeys.Orders).GetConfigurationStatus();

        Assert.False(status.IsConfigured);
        Assert.Equal("None", status.Mode);
    }

    [Fact]
    public void GetConfigurationStatus_PrefersGlobalConnectionString_WhenBothConfigured()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["AzureTableAccounts:table-orders:ServiceUri"] = "https://example.table.core.windows.net/",
            ["AzureTableConnectionString:ConnectionString"] = "UseDevelopmentStorage=true",
        }, AzureTableKeys.Orders);

        using var provider = services.BuildServiceProvider();
        var status = provider.GetRequiredKeyedService<IAzureTableService>(AzureTableKeys.Orders).GetConfigurationStatus();

        Assert.True(status.IsConfigured);
        Assert.Equal("ConnectionString", status.Mode);
    }

    [Fact]
    public void GetConfigurationStatus_UsesAccountUri_WhenOnlyAccountConfigured()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["AzureTableAccounts:table-orders:ServiceUri"] = "https://example.table.core.windows.net/",
        }, AzureTableKeys.Orders);

        using var provider = services.BuildServiceProvider();
        var status = provider.GetRequiredKeyedService<IAzureTableService>(AzureTableKeys.Orders).GetConfigurationStatus();

        Assert.True(status.IsConfigured);
        Assert.Equal("Uri", status.Mode);
    }

    [Fact]
    public void EmulatedConnectionStringService_IgnoresAccountUri_WhenGlobalConnectionStringConfigured()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["AzureTableAccounts:table-orders:ServiceUri"] = "https://example.table.core.windows.net/",
            ["AzureTableConnectionString:ConnectionString"] = "UseDevelopmentStorage=true",
        }, AzureTableKeys.Orders);

        using var provider = services.BuildServiceProvider();
        var status = AzureTableServiceFactories
            .CreateConnectionStringService(provider)
            .GetConfigurationStatus();

        Assert.True(status.IsConfigured);
        Assert.Equal("ConnectionString", status.Mode);
    }

    [Fact]
    public void AddAzureTableDictionary_ResolvesMemberServices()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["AzureTableAccounts:table-geo-us-east:ServiceUri"] = "https://us.example.table.core.windows.net/",
            ["AzureTableAccounts:table-geo-eu-west:ServiceUri"] = "https://eu.example.table.core.windows.net/",
        });

        services.AddAzureTableAccount(AzureTableKeys.GeoUsEast);
        services.AddAzureTableAccount(AzureTableKeys.GeoEuWest);
        services.AddAzureTableDictionary(
            AzureTableKeys.GeoReplicas,
            (AzureTableGeoKeys.UsEast, AzureTableKeys.GeoUsEast),
            (AzureTableGeoKeys.EuWest, AzureTableKeys.GeoEuWest));

        using var provider = services.BuildServiceProvider();
        var replicas = provider.GetRequiredKeyedService<IReadOnlyDictionary<string, IAzureTableService>>(AzureTableKeys.GeoReplicas);

        Assert.Equal("Uri", replicas[AzureTableGeoKeys.UsEast].GetConfigurationStatus().Mode);
        Assert.Equal("Uri", replicas[AzureTableGeoKeys.EuWest].GetConfigurationStatus().Mode);
    }

    private static ServiceCollection CreateServices(
        IReadOnlyDictionary<string, string?> settings,
        string? anchorKey = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddAzureTableStorage(configuration);

        if (anchorKey is not null)
        {
            services.AddAzureTableAccount(anchorKey);
        }

        return services;
    }
}
