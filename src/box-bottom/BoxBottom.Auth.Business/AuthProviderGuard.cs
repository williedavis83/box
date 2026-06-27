using BoxBottom.Auth.Contract;
using Microsoft.Extensions.Options;

namespace BoxBottom.Auth.Business;

public sealed class AuthProviderGuard(IOptions<AuthOptions> options) : IAuthProviderGuard
{
    public string ActiveProviderName => options.Value.Provider;

    public bool IsProviderActive(string providerName) =>
        string.Equals(ActiveProviderName, providerName, StringComparison.OrdinalIgnoreCase);

    public void EnsureProviderActive(string providerName)
    {
        if (!IsProviderActive(providerName))
        {
            throw new AuthProviderMismatchException(
                $"Auth provider '{providerName}' is not enabled for this application. Active provider is '{ActiveProviderName}'.");
        }
    }
}
