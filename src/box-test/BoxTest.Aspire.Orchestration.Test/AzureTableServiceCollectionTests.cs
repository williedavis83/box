using BoxBottom.Azure.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AzureTableServiceCollectionTests
{
    [Fact]
    public void GetConfigurationStatus_ReturnsNone_WhenUnconfigured()
    {
        var services = CreateServices(new Dictionary<string, string?>());
        using var provider = services.BuildServiceProvider();

        var status = provider.GetRequiredService<IAzureTableService>().GetConfigurationStatus();

        Assert.False(status.IsConfigured);
        Assert.Equal("None", status.Mode);
    }

    [Fact]
    public void GetConfigurationStatus_PrefersConnectionString_WhenBothConfigured()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["AzureTableUri:ServiceUri"] = "https://example.table.core.windows.net/",
            ["AzureTableConnectionString:ConnectionString"] = "UseDevelopmentStorage=true",
        });

        using var provider = services.BuildServiceProvider();
        var status = provider.GetRequiredService<IAzureTableService>().GetConfigurationStatus();

        Assert.True(status.IsConfigured);
        Assert.Equal("ConnectionString", status.Mode);
    }

    [Fact]
    public void GetConfigurationStatus_UsesUri_WhenOnlyUriConfigured()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["AzureTableUri:ServiceUri"] = "https://example.table.core.windows.net/",
        });

        using var provider = services.BuildServiceProvider();
        var status = provider.GetRequiredService<IAzureTableService>().GetConfigurationStatus();

        Assert.True(status.IsConfigured);
        Assert.Equal("Uri", status.Mode);
    }

    private static ServiceCollection CreateServices(IReadOnlyDictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddAzureTableStorage(configuration);
        return services;
    }
}
