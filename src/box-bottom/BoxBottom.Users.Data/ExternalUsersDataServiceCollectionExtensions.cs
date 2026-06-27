using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Models;
using BoxBottom.Users.Data;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Users.Data;

public static class ExternalUsersDataServiceCollectionExtensions
{
    public static IServiceCollection AddExternalUsersData<TExternalUser>(
        this IServiceCollection services,
        string tableAnchorKey)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableAnchorKey);

        services.AddSingleton<IExternalUserRepository<TExternalUser>>(serviceProvider =>
            new AzureTableExternalUserRepository<TExternalUser>(
                serviceProvider.GetRequiredKeyedService<Azure.Table.IAzureTableService>(tableAnchorKey),
                serviceProvider.GetRequiredService<IExternalUserMapper<TExternalUser>>()));

        return services;
    }
}
