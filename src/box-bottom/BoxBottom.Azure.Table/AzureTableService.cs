using Azure;
using Azure.Data.Tables;
using BoxBottom.Azure.Table.Internal;
using BoxBottom.Azure.Table.Models;

namespace BoxBottom.Azure.Table;

internal sealed class AzureTableService(IAzureTableServiceClientFactory clientFactory) : IAzureTableService
{
    private readonly IAzureTableServiceClientFactory _clientFactory =
        clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));

    public AzureTableConfigurationStatus GetConfigurationStatus()
    {
        var resolution = _clientFactory.Resolve();
        return new AzureTableConfigurationStatus(resolution.IsConfigured, resolution.Mode);
    }

    public async Task<IReadOnlyList<string>> ListTablesAsync(CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.CreateClient();
        var tableNames = new List<string>();

        await foreach (var table in client.QueryAsync(filter: (string?)null, cancellationToken: cancellationToken))
        {
            tableNames.Add(table.Name);
        }

        return tableNames;
    }

    public async Task CreateTableIfNotExistsAsync(string tableName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        var client = _clientFactory.CreateClient();
        await client.CreateTableIfNotExistsAsync(tableName, cancellationToken);
    }

    public async Task<AzureTableEntityResult?> GetEntityAsync(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync<TableEntity>(
            tableName,
            partitionKey,
            rowKey,
            cancellationToken);

        return entity is null ? null : ToResult(entity);
    }

    public async Task<TEntity?> GetEntityAsync<TEntity>(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(rowKey);

        var client = _clientFactory.CreateClient();
        var tableClient = client.GetTableClient(tableName);

        try
        {
            var response = await tableClient.GetEntityAsync<TEntity>(
                partitionKey,
                rowKey,
                cancellationToken: cancellationToken);

            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<AzureTableEntityResult>> QueryEntitiesAsync(
        string tableName,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        var client = _clientFactory.CreateClient();
        var tableClient = client.GetTableClient(tableName);
        var results = new List<AzureTableEntityResult>();

        await foreach (var entity in tableClient.QueryAsync<TableEntity>(cancellationToken: cancellationToken))
        {
            results.Add(ToResult(entity));

            if (maxResults is > 0 && results.Count >= maxResults)
            {
                break;
            }
        }

        return results;
    }

    public async Task UpsertEntityAsync(
        string tableName,
        AzureTableUpsertEntityRequest entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity.PartitionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity.RowKey);

        var tableEntity = new TableEntity(entity.PartitionKey, entity.RowKey);
        foreach (var (key, value) in entity.Properties)
        {
            tableEntity[key] = value;
        }

        await UpsertEntityAsync(tableName, tableEntity, cancellationToken);
    }

    public async Task UpsertEntityAsync<TEntity>(
        string tableName,
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : ITableEntity
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity.PartitionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity.RowKey);

        var client = _clientFactory.CreateClient();
        var tableClient = client.GetTableClient(tableName);
        await tableClient.UpsertEntityAsync(entity, cancellationToken: cancellationToken);
    }

    private static AzureTableEntityResult ToResult(TableEntity entity)
    {
        var properties = entity
            .Where(entry =>
                !string.Equals(entry.Key, "PartitionKey", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.Key, "RowKey", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.Key, "Timestamp", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.Key, "ETag", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(entry => entry.Key, entry => (object?)entry.Value, StringComparer.OrdinalIgnoreCase);

        return new AzureTableEntityResult(
            entity.PartitionKey,
            entity.RowKey,
            entity.Timestamp,
            properties);
    }
}
