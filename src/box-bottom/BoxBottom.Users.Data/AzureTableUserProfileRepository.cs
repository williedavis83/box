using BoxBottom.Azure.Table;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Entities;
using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Users.Data;

public sealed class AzureTableUserProfileRepository<TProfile, TTableEntity, TUpdateRequest>(
    IAzureTableService tableService,
    IUserProfileEntityMapper<TProfile, TTableEntity, TUpdateRequest> entityMapper)
    : IUserProfileRepository<TProfile, TUpdateRequest>
    where TProfile : BaseUserProfile
    where TTableEntity : BaseUserProfileTableEntity, new()
    where TUpdateRequest : BaseUpdateUserProfileRequest
{
    public async Task<TProfile?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var entity = await tableService.GetEntityAsync<TTableEntity>(
            UserProfileEntityKeys.TableName,
            UserProfileEntityKeys.BuildPartitionKey(userId),
            UserProfileEntityKeys.BuildRowKey(),
            cancellationToken);

        return entity is null ? null : entityMapper.MapFromEntity(entity);
    }

    public async Task<TProfile> CreateAsync(
        Guid userId,
        CreateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profile = entityMapper.CreateProfile(userId, request, DateTimeOffset.UtcNow);
        await UpsertProfileAsync(profile, cancellationToken);
        return profile;
    }

    public async Task<TProfile> UpdateAsync(
        Guid userId,
        TUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await GetAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User profile '{userId}' was not found.");

        var updated = entityMapper.ApplyUpdate(existing, request);
        await UpsertProfileAsync(updated, cancellationToken);
        return updated;
    }

    private Task UpsertProfileAsync(TProfile profile, CancellationToken cancellationToken) =>
        tableService.UpsertEntityAsync(
            UserProfileEntityKeys.TableName,
            entityMapper.MapToEntity(profile),
            cancellationToken);
}
