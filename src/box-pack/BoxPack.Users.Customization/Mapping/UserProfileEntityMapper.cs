using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Models;
using BoxPack.Users.Customization.Entities;
using BoxPack.Users.Customization.Models;

namespace BoxPack.Users.Customization.Mapping;

public sealed class UserProfileEntityMapper
    : IUserProfileEntityMapper<UserProfile, UserProfileTableEntity, UpdateUserProfileRequest>
{
    public UserProfile MapFromEntity(UserProfileTableEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserProfile(
            entity.UserId,
            entity.DisplayName,
            entity.Email,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);
    }

    public UserProfileTableEntity MapToEntity(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new UserProfileTableEntity
        {
            PartitionKey = UserProfileEntityKeys.BuildPartitionKey(profile.UserId),
            RowKey = UserProfileEntityKeys.BuildRowKey(),
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            Email = profile.Email,
            CreatedAtUtc = profile.CreatedAtUtc,
            UpdatedAtUtc = profile.UpdatedAtUtc,
        };
    }

    public UserProfile CreateProfile(
        Guid userId,
        CreateUserProfileRequest request,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new UserProfile(
            userId,
            request.DisplayName,
            request.Email,
            timestamp,
            timestamp);
    }

    public UserProfile ApplyUpdate(UserProfile existing, UpdateUserProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(request);

        return existing with
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };
    }
}
