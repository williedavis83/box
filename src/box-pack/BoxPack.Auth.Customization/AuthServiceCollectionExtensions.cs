using BoxBottom.Auth.Business;
using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Entra;
using BoxBottom.Auth.ZeroAuth;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Models;
using BoxBottom.Users.Data;
using BoxPack.Auth.Customization.Mapping;
using BoxPack.Users.Customization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoxPack.Auth.Customization;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddBoxPackAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        services.AddSingleton<IExternalUserMapper<ExternalUser>, ExternalUserIdentityMapper>();
        services.AddExternalUsersData<ExternalUser>(UserTableKeys.Users);
        services.AddAuthBusiness<UserProfile, UpdateUserProfileRequest>();

        services.AddConfiguredAuthProvider(
            configuration,
            [
                new EntraAuthProviderRegistration<UserProfile>(),
                new ZeroAuthAuthProviderRegistration(),
            ]);

        return services;
    }
}
