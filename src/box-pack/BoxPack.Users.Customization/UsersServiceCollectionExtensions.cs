using BoxBottom.Azure.Table;
using BoxBottom.Users.Business;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Data;
using BoxPack.Users.Customization.Entities;
using BoxPack.Users.Customization.Mapping;
using BoxPack.Users.Customization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoxPack.Users.Customization;

public static class UsersServiceCollectionExtensions
{
    public static IServiceCollection AddBoxPackUsers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddAzureTableStorage(configuration);
        services.AddAzureTableAccount(UserTableKeys.Users);
        services.AddSingleton<IUserProfileEntityMapper<UserProfile, UserProfileTableEntity, UpdateUserProfileRequest>, UserProfileEntityMapper>();
        services.AddUsersData<UserProfile, UserProfileTableEntity, UpdateUserProfileRequest>(UserTableKeys.Users);
        services.AddUsersBusiness<UserProfile, UpdateUserProfileRequest>();

        return services;
    }
}
