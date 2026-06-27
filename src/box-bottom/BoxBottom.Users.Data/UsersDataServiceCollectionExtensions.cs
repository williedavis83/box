using BoxBottom.Azure.Table;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Entities;
using BoxBottom.Users.Contract.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Users.Data;

public static class UsersDataServiceCollectionExtensions
{
    public static IServiceCollection AddUsersData<TProfile, TTableEntity, TUpdateRequest>(
        this IServiceCollection services,
        string tableAnchorKey)
        where TProfile : BaseUserProfile
        where TTableEntity : BaseUserProfileTableEntity, new()
        where TUpdateRequest : BaseUpdateUserProfileRequest
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableAnchorKey);

        services.AddSingleton<IUserProfileRepository<TProfile, TUpdateRequest>>(serviceProvider =>
            new AzureTableUserProfileRepository<TProfile, TTableEntity, TUpdateRequest>(
                serviceProvider.GetRequiredKeyedService<IAzureTableService>(tableAnchorKey),
                serviceProvider.GetRequiredService<IUserProfileEntityMapper<TProfile, TTableEntity, TUpdateRequest>>()));

        return services;
    }
}
