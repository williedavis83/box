using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Users.Contract;

public interface IUserProfileService<TProfile, TUpdateRequest>
    where TProfile : BaseUserProfile
    where TUpdateRequest : BaseUpdateUserProfileRequest
{
    Task<TProfile?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TProfile> CreateAsync(CreateUserProfileRequest request, CancellationToken cancellationToken = default);

    Task<TProfile> CreateForUserAsync(
        Guid userId,
        CreateUserProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<TProfile> UpdateAsync(Guid userId, TUpdateRequest request, CancellationToken cancellationToken = default);
}
