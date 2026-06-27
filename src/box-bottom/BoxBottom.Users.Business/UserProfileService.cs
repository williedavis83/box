using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Users.Business;

public sealed class UserProfileService<TProfile, TUpdateRequest>(
    IUserProfileRepository<TProfile, TUpdateRequest> repository)
    : IUserProfileService<TProfile, TUpdateRequest>
    where TProfile : BaseUserProfile
    where TUpdateRequest : BaseUpdateUserProfileRequest
{
    public Task<TProfile?> GetAsync(Guid userId, CancellationToken cancellationToken = default) =>
        repository.GetAsync(userId, cancellationToken);

    public Task<TProfile> CreateAsync(CreateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return repository.CreateAsync(Guid.NewGuid(), request, cancellationToken);
    }

    public Task<TProfile> CreateForUserAsync(
        Guid userId,
        CreateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return repository.CreateAsync(userId, request, cancellationToken);
    }

    public Task<TProfile> UpdateAsync(
        Guid userId,
        TUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return repository.UpdateAsync(userId, request, cancellationToken);
    }
}
