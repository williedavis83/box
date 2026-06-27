using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Users.Business;

public static class UsersBusinessServiceCollectionExtensions
{
    public static IServiceCollection AddUsersBusiness<TProfile, TUpdateRequest>(this IServiceCollection services)
        where TProfile : BaseUserProfile
        where TUpdateRequest : BaseUpdateUserProfileRequest
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IUserProfileService<TProfile, TUpdateRequest>, UserProfileService<TProfile, TUpdateRequest>>();
        return services;
    }
}
