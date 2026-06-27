using BoxBottom.Auth.Contract;
using BoxBottom.Users.Contract.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Auth.Entra;

public sealed class EntraAuthProviderRegistration<TProfile> : IAuthProviderRegistration
    where TProfile : BaseUserProfile
{
    public string ProviderName => EntraAuthProvider.Name;

    public void Register(IServiceCollection services, IConfiguration configuration) =>
        services.AddEntraAuth<TProfile>(configuration);
}
