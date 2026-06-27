using BoxBottom.Auth.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Auth.ZeroAuth;

public sealed class ZeroAuthAuthProviderRegistration : IAuthProviderRegistration
{
    public string ProviderName => ZeroAuthAuthProvider.Name;

    public void Register(IServiceCollection services, IConfiguration configuration) =>
        services.AddZeroAuth(configuration);
}
