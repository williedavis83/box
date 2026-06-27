using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Auth.Contract;

public interface IAuthProviderRegistration
{
    string ProviderName { get; }

    void Register(IServiceCollection services, IConfiguration configuration);
}
