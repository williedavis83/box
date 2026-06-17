using Azure.Data.Tables;
using Azure.Identity;
using BoxBottom.Azure.Table.Options;
using Microsoft.Extensions.Options;

namespace BoxBottom.Azure.Table.Internal;

internal sealed class AzureTableServiceClientFactory : IAzureTableServiceClientFactory
{
    private readonly AzureTableUriOptions _uriOptions;
    private readonly AzureTableConnectionStringOptions _connectionStringOptions;

    public AzureTableServiceClientFactory(
        IOptions<AzureTableUriOptions> uriOptions,
        IOptions<AzureTableConnectionStringOptions> connectionStringOptions)
    {
        ArgumentNullException.ThrowIfNull(uriOptions);
        ArgumentNullException.ThrowIfNull(connectionStringOptions);

        _uriOptions = uriOptions.Value;
        _connectionStringOptions = connectionStringOptions.Value;
    }

    public AzureTableConfigurationResolution Resolve()
    {
        if (!string.IsNullOrWhiteSpace(_connectionStringOptions.ConnectionString))
        {
            return new AzureTableConfigurationResolution(true, "ConnectionString");
        }

        if (_uriOptions.ServiceUri is not null)
        {
            return new AzureTableConfigurationResolution(true, "Uri");
        }

        return new AzureTableConfigurationResolution(false, "None");
    }

    public TableServiceClient CreateClient()
    {
        var resolution = Resolve();
        if (!resolution.IsConfigured)
        {
            throw new InvalidOperationException(
                "Azure Table Storage is not configured. Set either " +
                $"'{AzureTableConnectionStringOptions.SectionName}:ConnectionString' or " +
                $"'{AzureTableUriOptions.SectionName}:ServiceUri'.");
        }

        if (string.Equals(resolution.Mode, "ConnectionString", StringComparison.Ordinal))
        {
            return new TableServiceClient(_connectionStringOptions.ConnectionString);
        }

        return new TableServiceClient(_uriOptions.ServiceUri!, new DefaultAzureCredential());
    }
}
