using BoxBottom.Azure.Table;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Entities;
using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Users.Data;

public sealed class AzureTableExternalUserRepository<TExternalUser>(
    IAzureTableService tableService,
    IExternalUserMapper<TExternalUser> externalUserMapper)
    : IExternalUserRepository<TExternalUser>
{
    public async Task<Guid> GetOrCreateInternalUserIdAsync(
        TExternalUser externalUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(externalUser);

        var normalized = externalUserMapper.Map(externalUser);
        var partitionKey = ExternalUserEntityKeys.BuildPartitionKey(normalized.Provider);
        var rowKey = ExternalUserEntityKeys.BuildRowKey(normalized.ExternalId);

        var existing = await tableService.GetEntityAsync<ExternalUserTableEntity>(
            ExternalUserEntityKeys.TableName,
            partitionKey,
            rowKey,
            cancellationToken);

        if (existing is not null)
        {
            return existing.InternalUserId;
        }

        var internalUserId = Guid.NewGuid();
        var associatedAtUtc = DateTimeOffset.UtcNow;

        await tableService.UpsertEntityAsync(
            ExternalUserEntityKeys.TableName,
            new ExternalUserTableEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                InternalUserId = internalUserId,
                AssociatedAtUtc = associatedAtUtc,
            },
            cancellationToken);

        return internalUserId;
    }
}
