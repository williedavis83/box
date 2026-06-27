using BoxBottom.Auth.Contract;
using BoxBottom.Users.Contract.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Auth.Business;

public static class AuthBusinessServiceCollectionExtensions
{
    public static IServiceCollection AddAuthBusiness<TProfile, TUpdateRequest>(this IServiceCollection services)
        where TProfile : BaseUserProfile
        where TUpdateRequest : BaseUpdateUserProfileRequest
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthProviderGuard, AuthProviderGuard>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<IAuthSessionSignIn, AuthSessionSignIn>();
        services.AddScoped<IAuthService<TProfile>, AuthService<TProfile, TUpdateRequest>>();

        return services;
    }
}
