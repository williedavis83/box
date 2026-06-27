using BoxBottom.Auth.Contract.Models;

namespace BoxBottom.Auth.Contract;

public interface IAuthSessionSignIn
{
    Task SignInAsync(AuthSessionInfo session, CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);
}
