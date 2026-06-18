using BoxBottom.Azure.Table;
using Foo.Primary.Shared.AzureTable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foo.Primary.Api.AzureTable;

public static class AzureTableServiceCollectionExtensions
{
    public static IServiceCollection AddFooAzureTableScenarios(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddAzureTableStorage(configuration);
        services.AddAzureTableAccount(AzureTableKeys.Orders);
        services.AddAzureTableAccount(AzureTableKeys.Analytics);
        services.AddAzureTableAccount(AzureTableKeys.GeoUsEast);
        services.AddAzureTableAccount(AzureTableKeys.GeoEuWest);
        services.AddAzureTableDictionary(
            AzureTableKeys.GeoReplicas,
            (AzureTableGeoKeys.UsEast, AzureTableKeys.GeoUsEast),
            (AzureTableGeoKeys.EuWest, AzureTableKeys.GeoEuWest));

        return services;
    }
}
