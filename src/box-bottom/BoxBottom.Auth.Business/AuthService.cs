using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Contract.Models;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Auth.Business;

public sealed class AuthService<TProfile, TUpdateRequest>(
    IExternalUserRepository<ExternalUser> externalUserRepository,
    IUserProfileService<TProfile, TUpdateRequest> userProfileService,
    IAuthSessionSignIn sessionSignIn,
    ICurrentUserAccessor currentUserAccessor)
    : IAuthService<TProfile>
    where TProfile : BaseUserProfile
    where TUpdateRequest : BaseUpdateUserProfileRequest
{
    public async Task<AuthSessionInfo> SignInAsync(
        ExternalUser externalUser,
        ProfileSeed profileSeed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(externalUser);
        ArgumentNullException.ThrowIfNull(profileSeed);

        var internalUserId = await externalUserRepository.GetOrCreateInternalUserIdAsync(
            externalUser,
            cancellationToken);

        var profile = await userProfileService.GetAsync(internalUserId, cancellationToken);
        if (profile is null)
        {
            profile = await userProfileService.CreateForUserAsync(
                internalUserId,
                new CreateUserProfileRequest(profileSeed.DisplayName, profileSeed.Email),
                cancellationToken);
        }

        var session = AuthSessionInfoFactory.FromProfile(profile);
        await sessionSignIn.SignInAsync(session, cancellationToken);
        return session;
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default) =>
        sessionSignIn.SignOutAsync(cancellationToken);

    public async Task<AuthSessionInfo?> GetSessionAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUserAccessor.IsAuthenticated || currentUserAccessor.UserId is not Guid userId)
        {
            return null;
        }

        var profile = await userProfileService.GetAsync(userId, cancellationToken);
        return profile is null ? null : AuthSessionInfoFactory.FromProfile(profile);
    }
}
