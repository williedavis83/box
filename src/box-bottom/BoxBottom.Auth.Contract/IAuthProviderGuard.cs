namespace BoxBottom.Auth.Contract;

public interface IAuthProviderGuard
{
    string ActiveProviderName { get; }

    bool IsProviderActive(string providerName);

    void EnsureProviderActive(string providerName);
}
