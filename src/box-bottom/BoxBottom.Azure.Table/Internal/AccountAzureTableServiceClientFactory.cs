using Azure.Data.Tables;
using Azure.Identity;
using BoxBottom.Azure.Table.Options;
using Microsoft.Extensions.Options;

namespace BoxBottom.Azure.Table.Internal;

internal sealed class AccountAzureTableServiceClientFactory : IAzureTableServiceClientFactory
{
    private readonly string _accountKey;
    private readonly AzureTableConnectionStringOptions _globalConnectionStringOptions;
    private readonly AzureTableAccountsOptions _accountsOptions;

    public AccountAzureTableServiceClientFactory(
        string accountKey,
        IOptions<AzureTableConnectionStringOptions> globalConnectionStringOptions,
        IOptions<AzureTableAccountsOptions> accountsOptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountKey);
        ArgumentNullException.ThrowIfNull(globalConnectionStringOptions);
        ArgumentNullException.ThrowIfNull(accountsOptions);

        _accountKey = accountKey;
        _globalConnectionStringOptions = globalConnectionStringOptions.Value;
        _accountsOptions = accountsOptions.Value;
    }

    public AzureTableConfigurationResolution Resolve()
    {
        if (!string.IsNullOrWhiteSpace(_globalConnectionStringOptions.ConnectionString))
        {
            return new AzureTableConfigurationResolution(true, "ConnectionString");
        }

        if (_accountsOptions.TryGetValue(_accountKey, out var account))
        {
            if (!string.IsNullOrWhiteSpace(account.ConnectionString))
            {
                return new AzureTableConfigurationResolution(true, "ConnectionString");
            }

            if (account.ServiceUri is not null)
            {
                return new AzureTableConfigurationResolution(true, "Uri");
            }
        }

        return new AzureTableConfigurationResolution(false, "None");
    }

    public TableServiceClient CreateClient()
    {
        if (!string.IsNullOrWhiteSpace(_globalConnectionStringOptions.ConnectionString))
        {
            return new TableServiceClient(_globalConnectionStringOptions.ConnectionString);
        }

        if (!_accountsOptions.TryGetValue(_accountKey, out var account))
        {
            throw new InvalidOperationException(
                $"Azure Table account '{_accountKey}' is not configured under '{AzureTableAccountsOptions.SectionName}'.");
        }

        if (!string.IsNullOrWhiteSpace(account.ConnectionString))
        {
            return new TableServiceClient(account.ConnectionString);
        }

        if (account.ServiceUri is not null)
        {
            return new TableServiceClient(account.ServiceUri, new DefaultAzureCredential());
        }

        throw new InvalidOperationException(
            $"Azure Table account '{_accountKey}' has no connection string or service URI configured.");
    }
}
