using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Contract.Models;
using BoxBottom.Users.Contract.Models;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AuthSessionInfoFactoryTests
{
    [Fact]
    public void FromProfile_BuildsInitialsFromDisplayName()
    {
        var profile = new TestProfile(
            Guid.NewGuid(),
            "Test User",
            "test@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        var session = AuthSessionInfoFactory.FromProfile(profile);

        Assert.Equal("TU", session.Initials);
        Assert.Equal("Test User", session.DisplayName);
    }

    private sealed record TestProfile(
        Guid UserId,
        string DisplayName,
        string Email,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc)
        : BaseUserProfile(UserId, DisplayName, Email, CreatedAtUtc, UpdatedAtUtc);
}
