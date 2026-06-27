using BoxBottom.AzureTable.Emulation;
using BoxBottom.Azure.Table;
using BoxBottom.Emulation;
using BoxBottom.Users.Contract;
using BoxPack.Users.Customization;
using BoxPack.Users.Customization.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class UsersEmulationStartupTests
{
    [Fact]
    public void ApplyConfiguredEmulation_ActivatesUsersTableAnchor()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AzureTableAccounts:table-users:ServiceUri"] = "https://example.table.core.windows.net/",
            ["AzureTableConnectionString:ConnectionString"] = "UseDevelopmentStorage=true",
            ["AzureTable_Emulation"] =
                """
                {
                  "singletons": {
                    "table-users": {}
                  }
                }
                """,
        });

        builder.Services.AddBoxPackUsers(builder.Configuration);
        builder.AddEmulation(typeof(UserProfileController).Assembly)
            .RegisterAzureTableEmulation()
            .ApplyConfiguredEmulation();

        using var provider = builder.Services.BuildServiceProvider();
        var repository = provider.GetRequiredService<IEmulationAnchorRepository>();
        var usersTable = provider.GetRequiredKeyedService<IAzureTableService>(UserTableKeys.Users);

        Assert.True(repository.IsActivated(UserTableKeys.Users));
        Assert.Equal("ConnectionString", usersTable.GetConfigurationStatus().Mode);
    }
}
