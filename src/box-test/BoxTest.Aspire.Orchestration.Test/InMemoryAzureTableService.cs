using Azure.Data.Tables;
using BoxBottom.Azure.Table;
using BoxBottom.Azure.Table.Models;

namespace BoxTest.Aspire.Orchestration.Test;

internal sealed class InMemoryAzureTableService : IAzureTableService
{
    private readonly Dictionary<(string Table, string PartitionKey, string RowKey), object> _entities = new();

    public List<object> UpsertedEntities { get; } = [];

    public void SeedEntity(string tableName, object entity)
    {
        if (entity is not ITableEntity tableEntity)
        {
            throw new InvalidOperationException("Seeded entities must implement ITableEntity.");
        }

        _entities[(tableName, tableEntity.PartitionKey, tableEntity.RowKey)] = entity;
    }

    public AzureTableConfigurationStatus GetConfigurationStatus() =>
        new(true, "ConnectionString");

    public Task<IReadOnlyList<string>> ListTablesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<string>>([]);

    public Task CreateTableIfNotExistsAsync(string tableName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<AzureTableEntityResult?> GetEntityAsync(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<AzureTableEntityResult?>(null);

    public Task<TEntity?> GetEntityAsync<TEntity>(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity, new()
    {
        if (_entities.TryGetValue((tableName, partitionKey, rowKey), out var entity)
            && entity is TEntity typedEntity)
        {
            return Task.FromResult<TEntity?>(typedEntity);
        }

        return Task.FromResult<TEntity?>(null);
    }

    public Task<IReadOnlyList<AzureTableEntityResult>> QueryEntitiesAsync(
        string tableName,
        int? maxResults = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AzureTableEntityResult>>([]);

    public Task UpsertEntityAsync(
        string tableName,
        AzureTableUpsertEntityRequest entity,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpsertEntityAsync<TEntity>(
        string tableName,
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : ITableEntity
    {
        ArgumentNullException.ThrowIfNull(entity);
        UpsertedEntities.Add(entity);
        _entities[(tableName, entity.PartitionKey, entity.RowKey)] = entity!;
        return Task.CompletedTask;
    }
}
