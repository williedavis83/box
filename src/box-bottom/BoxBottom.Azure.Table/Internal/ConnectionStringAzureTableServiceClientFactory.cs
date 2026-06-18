using Azure.Data.Tables;
using BoxBottom.Azure.Table.Options;
using Microsoft.Extensions.Options;

namespace BoxBottom.Azure.Table.Internal;

internal sealed class ConnectionStringAzureTableServiceClientFactory : IAzureTableServiceClientFactory
{
    private readonly AzureTableConnectionStringOptions _connectionStringOptions;

    public ConnectionStringAzureTableServiceClientFactory(
        IOptions<AzureTableConnectionStringOptions> connectionStringOptions)
    {
        ArgumentNullException.ThrowIfNull(connectionStringOptions);
        _connectionStringOptions = connectionStringOptions.Value;
    }

    public AzureTableConfigurationResolution Resolve()
    {
        if (string.IsNullOrWhiteSpace(_connectionStringOptions.ConnectionString))
        {
            return new AzureTableConfigurationResolution(false, "None");
        }

        return new AzureTableConfigurationResolution(true, "ConnectionString");
    }

    public TableServiceClient CreateClient()
    {
        if (string.IsNullOrWhiteSpace(_connectionStringOptions.ConnectionString))
        {
            throw new InvalidOperationException(
                $"Azure Table Storage connection string is not configured. Set " +
                $"'{AzureTableConnectionStringOptions.SectionName}:ConnectionString'.");
        }

        return new TableServiceClient(_connectionStringOptions.ConnectionString);
    }
}
