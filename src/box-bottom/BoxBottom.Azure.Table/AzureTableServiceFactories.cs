using BoxBottom.Azure.Table.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BoxBottom.Azure.Table;

public static class AzureTableServiceFactories
{
    public static IAzureTableService CreateConnectionStringService(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var connectionStringOptions = serviceProvider.GetRequiredService<IOptions<Options.AzureTableConnectionStringOptions>>();
        var factory = new ConnectionStringAzureTableServiceClientFactory(connectionStringOptions);
        return new AzureTableService(factory);
    }
}
