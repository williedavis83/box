using Azure.Data.Tables;
using BoxBottom.Azure.Table.Models;

namespace BoxBottom.Azure.Table;

public interface IAzureTableService
{
    AzureTableConfigurationStatus GetConfigurationStatus();

    Task<IReadOnlyList<string>> ListTablesAsync(CancellationToken cancellationToken = default);

    Task CreateTableIfNotExistsAsync(string tableName, CancellationToken cancellationToken = default);

    Task<AzureTableEntityResult?> GetEntityAsync(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetEntityAsync<TEntity>(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity, new();

    Task<IReadOnlyList<AzureTableEntityResult>> QueryEntitiesAsync(
        string tableName,
        int? maxResults = null,
        CancellationToken cancellationToken = default);

    Task UpsertEntityAsync(
        string tableName,
        AzureTableUpsertEntityRequest entity,
        CancellationToken cancellationToken = default);

    Task UpsertEntityAsync<TEntity>(
        string tableName,
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : ITableEntity;
}
