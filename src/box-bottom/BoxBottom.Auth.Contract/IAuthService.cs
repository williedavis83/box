using BoxBottom.Auth.Contract.Models;
using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Auth.Contract;

public interface IAuthService<TProfile>
    where TProfile : BaseUserProfile
{
    Task<AuthSessionInfo> SignInAsync(
        ExternalUser externalUser,
        ProfileSeed profileSeed,
        CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);

    Task<AuthSessionInfo?> GetSessionAsync(CancellationToken cancellationToken = default);
}
