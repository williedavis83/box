using BoxBottom.Users.Contract.Entities;
using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Users.Contract;

public interface IUserProfileEntityMapper<TProfile, TTableEntity, TUpdateRequest>
    where TProfile : BaseUserProfile
    where TTableEntity : BaseUserProfileTableEntity, new()
    where TUpdateRequest : BaseUpdateUserProfileRequest
{
    TProfile MapFromEntity(TTableEntity entity);

    TTableEntity MapToEntity(TProfile profile);

    TProfile CreateProfile(Guid userId, CreateUserProfileRequest request, DateTimeOffset timestamp);

    TProfile ApplyUpdate(TProfile existing, TUpdateRequest request);
}
