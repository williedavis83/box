using BoxBottom.Users.Business;
using BoxBottom.Users.Contract.Models;
using BoxBottom.Users.Data;
using BoxPack.Users.Customization.Entities;
using BoxPack.Users.Customization.Mapping;
using BoxPack.Users.Customization.Models;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class UserProfileServiceTests
{
    [Fact]
    public async Task CreateForUserAsync_UsesSuppliedUserId()
    {
        var tableService = new InMemoryAzureTableService();
        var repository = new AzureTableUserProfileRepository<UserProfile, UserProfileTableEntity, UpdateUserProfileRequest>(
            tableService,
            new UserProfileEntityMapper());
        var service = new UserProfileService<UserProfile, UpdateUserProfileRequest>(repository);
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var profile = await service.CreateForUserAsync(
            userId,
            new CreateUserProfileRequest("Assigned User", "assigned@example.com"),
            CancellationToken.None);

        Assert.Equal(userId, profile.UserId);
    }

    [Fact]
    public async Task UpdateAsync_UsesMapperToApplyUpdateRequest()
    {
        var tableService = new InMemoryAzureTableService();
        var repository = new AzureTableUserProfileRepository<UserProfile, UserProfileTableEntity, UpdateUserProfileRequest>(
            tableService,
            new UserProfileEntityMapper());
        var service = new UserProfileService<UserProfile, UpdateUserProfileRequest>(repository);
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        await service.CreateForUserAsync(
            userId,
            new CreateUserProfileRequest("Original User", "original@example.com"),
            CancellationToken.None);

        var updated = await service.UpdateAsync(
            userId,
            new UpdateUserProfileRequest("Updated User", "updated@example.com"),
            CancellationToken.None);

        Assert.Equal("Updated User", updated.DisplayName);
        Assert.Equal("updated@example.com", updated.Email);
    }
}
