using BoxBottom.Azure.Table.Internal;
using BoxBottom.Azure.Table.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Azure.Table;

public static class AzureTableServiceCollectionExtensions
{
    public static IServiceCollection AddAzureTableStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AzureTableUriOptions>(
            configuration.GetSection(AzureTableUriOptions.SectionName));
        services.Configure<AzureTableConnectionStringOptions>(
            configuration.GetSection(AzureTableConnectionStringOptions.SectionName));

        services.AddSingleton<IAzureTableServiceClientFactory, AzureTableServiceClientFactory>();
        services.AddSingleton<IAzureTableService, AzureTableService>();

        return services;
    }
}
