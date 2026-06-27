using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Contract.Models;
using BoxBottom.Auth.Entra;
using BoxBottom.Auth.ZeroAuth;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Models;
using BoxPack.Auth.Customization;
using BoxPack.Users.Customization;
using BoxPack.Users.Customization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AuthServiceCollectionTests
{
    [Fact]
    public void AddBoxPackAuth_RegistersZeroAuthServices()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["Auth:Provider"] = ZeroAuthAuthProvider.Name,
            ["AzureTableAccounts:table-users:ServiceUri"] = "https://example.table.core.windows.net/",
        });

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IAuthService<UserProfile>>());
        Assert.NotNull(provider.GetRequiredService<IExternalUserRepository<ExternalUser>>());
        Assert.NotNull(provider.GetRequiredService<IZeroAuthRequestValidator>());
        Assert.Equal(
            ZeroAuthAuthProvider.Name,
            provider.GetRequiredService<IAuthProviderGuard>().ActiveProviderName);
    }

    [Fact]
    public void AddBoxPackAuth_RegistersEntraServices()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["Auth:Provider"] = EntraAuthProvider.Name,
            ["Auth:Entra:TenantId"] = "tenant-id",
            ["Auth:Entra:ClientId"] = "client-id",
            ["Auth:Entra:ClientSecret"] = "client-secret",
            ["AzureTableAccounts:table-users:ServiceUri"] = "https://example.table.core.windows.net/",
        });

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IAuthService<UserProfile>>());
        Assert.NotNull(provider.GetRequiredService<IEntraClaimsMapper>());
        Assert.Equal(
            EntraAuthProvider.Name,
            provider.GetRequiredService<IAuthProviderGuard>().ActiveProviderName);
    }

    [Fact]
    public void AddConfiguredAuthProvider_Throws_WhenProviderIsNotRegistered()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Provider"] = "UnknownProvider",
                ["AzureTableAccounts:table-users:ServiceUri"] = "https://example.table.core.windows.net/",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddBoxPackUsers(configuration);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddBoxPackAuth(configuration));

        Assert.Contains("UnknownProvider", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ZeroAuthRequestValidator_RejectsDisallowedProvider()
    {
        var services = CreateServices(new Dictionary<string, string?>
        {
            ["Auth:Provider"] = ZeroAuthAuthProvider.Name,
            ["Auth:ZeroAuth:AllowedProviders:0"] = ZeroAuthAuthProvider.Name,
            ["AzureTableAccounts:table-users:ServiceUri"] = "https://example.table.core.windows.net/",
        });

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IZeroAuthRequestValidator>();

        var error = validator.Validate(new ZeroAuthLoginRequest(
            EntraAuthProvider.Name,
            "oid-123",
            "User",
            "user@example.com"));

        Assert.NotNull(error);
    }

    private static ServiceCollection CreateServices(IReadOnlyDictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddBoxPackUsers(configuration);
        services.AddBoxPackAuth(configuration);
        return services;
    }
}
