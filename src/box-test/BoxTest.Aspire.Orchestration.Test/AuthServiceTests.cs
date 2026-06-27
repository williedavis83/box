using BoxBottom.Auth.Business;
using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Contract.Models;
using BoxBottom.Users.Business;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Entities;
using BoxBottom.Users.Contract.Models;
using BoxBottom.Users.Data;
using BoxPack.Users.Customization.Entities;
using BoxPack.Users.Customization.Mapping;
using BoxPack.Users.Customization.Models;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AuthServiceTests
{
    [Fact]
    public async Task SignInAsync_CreatesProfile_WhenExternalUserIsNew()
    {
        var tableService = new InMemoryAzureTableService();
        var profileRepository = CreateProfileRepository(tableService);
        var profileService = new UserProfileService<UserProfile, UpdateUserProfileRequest>(profileRepository);
        var externalUserRepository = new AzureTableExternalUserRepository<ExternalUser>(
            tableService,
            new ExternalUserIdentityMapper());
        var sessionSignIn = new RecordingAuthSessionSignIn();
        var authService = new AuthService<UserProfile, UpdateUserProfileRequest>(
            externalUserRepository,
            profileService,
            sessionSignIn,
            new AnonymousCurrentUserAccessor());

        var session = await authService.SignInAsync(
            new ExternalUser("ZeroAuth", "user-1"),
            new ProfileSeed("Test User", "test@example.com"),
            CancellationToken.None);

        Assert.Equal("Test User", session.DisplayName);
        Assert.Equal("TU", session.Initials);
        Assert.NotEqual(Guid.Empty, session.UserId);
        Assert.Equal(session.UserId, sessionSignIn.LastSession?.UserId);

        var profile = await profileService.GetAsync(session.UserId, CancellationToken.None);
        Assert.NotNull(profile);
        Assert.Equal("test@example.com", profile!.Email);
    }

    [Fact]
    public async Task SignInAsync_ReusesExistingProfile_WhenExternalUserAlreadyMapped()
    {
        var tableService = new InMemoryAzureTableService();
        var profileRepository = CreateProfileRepository(tableService);
        var profileService = new UserProfileService<UserProfile, UpdateUserProfileRequest>(profileRepository);
        var existingUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        await profileService.CreateForUserAsync(
            existingUserId,
            new CreateUserProfileRequest("Existing User", "existing@example.com"),
            CancellationToken.None);

        tableService.SeedEntity(
            ExternalUserEntityKeys.TableName,
            new ExternalUserTableEntity
            {
                PartitionKey = "zeroauth",
                RowKey = "user-1",
                InternalUserId = existingUserId,
                AssociatedAtUtc = DateTimeOffset.UtcNow,
            });

        var externalUserRepository = new AzureTableExternalUserRepository<ExternalUser>(
            tableService,
            new ExternalUserIdentityMapper());
        var sessionSignIn = new RecordingAuthSessionSignIn();
        var authService = new AuthService<UserProfile, UpdateUserProfileRequest>(
            externalUserRepository,
            profileService,
            sessionSignIn,
            new AnonymousCurrentUserAccessor());

        var session = await authService.SignInAsync(
            new ExternalUser("ZeroAuth", "user-1"),
            new ProfileSeed("Ignored", "ignored@example.com"),
            CancellationToken.None);

        Assert.Equal(existingUserId, session.UserId);
        Assert.Equal("Existing User", session.DisplayName);
    }

    private static AzureTableUserProfileRepository<UserProfile, UserProfileTableEntity, UpdateUserProfileRequest>
        CreateProfileRepository(InMemoryAzureTableService tableService) =>
        new(tableService, new UserProfileEntityMapper());

    private sealed class ExternalUserIdentityMapper : IExternalUserMapper<ExternalUser>
    {
        public ExternalUser Map(ExternalUser externalUser) => externalUser;
    }

    private sealed class RecordingAuthSessionSignIn : IAuthSessionSignIn
    {
        public AuthSessionInfo? LastSession { get; private set; }

        public Task SignInAsync(AuthSessionInfo session, CancellationToken cancellationToken = default)
        {
            LastSession = session;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class AnonymousCurrentUserAccessor : ICurrentUserAccessor
    {
        public Guid? UserId => null;

        public bool IsAuthenticated => false;
    }
}
