using BoxBottom.Azure.Table;
using BoxBottom.Users.Contract;
using BoxPack.Users.Customization;
using BoxPack.Users.Customization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class UsersServiceCollectionTests
{
    [Fact]
    public void AddBoxPackUsers_ResolvesKeyedUsersTableService()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["AzureTableAccounts:table-users:ServiceUri"] = "https://example.table.core.windows.net/",
        });

        using var provider = services.BuildServiceProvider();
        var status = provider
            .GetRequiredKeyedService<IAzureTableService>(UserTableKeys.Users)
            .GetConfigurationStatus();

        Assert.True(status.IsConfigured);
        Assert.Equal("Uri", status.Mode);
    }

    [Fact]
    public void AddBoxPackUsers_ResolvesUserProfileService()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["AzureTableAccounts:table-users:ServiceUri"] = "https://example.table.core.windows.net/",
        });

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IUserProfileService<UserProfile, UpdateUserProfileRequest>>());
        Assert.NotNull(provider.GetRequiredService<IUserProfileRepository<UserProfile, UpdateUserProfileRequest>>());
    }

    private static ServiceCollection CreateServices(IReadOnlyDictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddBoxPackUsers(configuration);
        return services;
    }
}
