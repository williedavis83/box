using BoxBottom.Users.Contract;
using BoxPack.Users.Customization.Entities;
using BoxPack.Users.Customization.Mapping;
using BoxPack.Users.Customization.Models;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class UserProfileEntityMapperTests
{
    private readonly UserProfileEntityMapper _mapper = new();

    [Fact]
    public void MapFromEntity_MapsAllProperties()
    {
        var userId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
        var created = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var updated = new DateTimeOffset(2024, 6, 7, 8, 9, 10, TimeSpan.Zero);

        var profile = _mapper.MapFromEntity(new UserProfileTableEntity
        {
            PartitionKey = UserProfileEntityKeys.BuildPartitionKey(userId),
            RowKey = UserProfileEntityKeys.BuildRowKey(),
            UserId = userId,
            DisplayName = "Ada Lovelace",
            Email = "ada@example.com",
            CreatedAtUtc = created,
            UpdatedAtUtc = updated,
        });

        Assert.Equal(userId, profile.UserId);
        Assert.Equal("Ada Lovelace", profile.DisplayName);
        Assert.Equal("ada@example.com", profile.Email);
        Assert.Equal(created, profile.CreatedAtUtc);
        Assert.Equal(updated, profile.UpdatedAtUtc);
    }

    [Fact]
    public void MapToEntity_MapsAllPropertiesAndKeys()
    {
        var userId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
        var created = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var updated = new DateTimeOffset(2024, 6, 7, 8, 9, 10, TimeSpan.Zero);

        var entity = _mapper.MapToEntity(new UserProfile(
            userId,
            "Ada Lovelace",
            "ada@example.com",
            created,
            updated));

        Assert.Equal(UserProfileEntityKeys.BuildPartitionKey(userId), entity.PartitionKey);
        Assert.Equal(UserProfileEntityKeys.BuildRowKey(), entity.RowKey);
        Assert.Equal(userId, entity.UserId);
        Assert.Equal("Ada Lovelace", entity.DisplayName);
        Assert.Equal("ada@example.com", entity.Email);
        Assert.Equal(created, entity.CreatedAtUtc);
        Assert.Equal(updated, entity.UpdatedAtUtc);
    }
}
