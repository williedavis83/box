using BoxBottom.Auth.Business;
using BoxBottom.Auth.Contract;
using Microsoft.Extensions.Options;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AuthProviderGuardTests
{
    [Fact]
    public void EnsureProviderActive_Throws_WhenProviderIsNotActive()
    {
        var guard = CreateGuard(ZeroAuthAuthProvider.Name);

        var exception = Assert.Throws<AuthProviderMismatchException>(() =>
            guard.EnsureProviderActive(EntraAuthProvider.Name));

        Assert.Contains(EntraAuthProvider.Name, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsureProviderActive_Succeeds_WhenProviderIsActive()
    {
        var guard = CreateGuard(EntraAuthProvider.Name);
        guard.EnsureProviderActive(EntraAuthProvider.Name);
    }

    [Fact]
    public void IsProviderActive_IsCaseInsensitive()
    {
        var guard = CreateGuard("entra");

        Assert.True(guard.IsProviderActive(EntraAuthProvider.Name));
    }

    private static AuthProviderGuard CreateGuard(string provider) =>
        new(Options.Create(new AuthOptions { Provider = provider }));

    private static class EntraAuthProvider
    {
        public const string Name = "Entra";
    }

    private static class ZeroAuthAuthProvider
    {
        public const string Name = "ZeroAuth";
    }
}
