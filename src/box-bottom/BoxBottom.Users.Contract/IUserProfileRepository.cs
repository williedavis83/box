using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Users.Contract;

public interface IUserProfileRepository<TProfile, TUpdateRequest>
    where TProfile : BaseUserProfile
    where TUpdateRequest : BaseUpdateUserProfileRequest
{
    Task<TProfile?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TProfile> CreateAsync(
        Guid userId,
        CreateUserProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<TProfile> UpdateAsync(
        Guid userId,
        TUpdateRequest request,
        CancellationToken cancellationToken = default);
}
