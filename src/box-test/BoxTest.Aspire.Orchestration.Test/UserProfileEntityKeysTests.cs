using BoxBottom.Users.Contract;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class UserProfileEntityKeysTests
{
    [Fact]
    public void BuildPartitionKey_ReversesGuidWithoutDashes()
    {
        var userId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");

        var partitionKey = UserProfileEntityKeys.BuildPartitionKey(userId);

        Assert.Equal("fedcba9876543210fedcba9876543210", partitionKey);
        Assert.NotEqual(userId.ToString("D"), partitionKey);
    }

    [Fact]
    public void BuildPartitionKey_ProducesDistinctValuesForDistinctUsers()
    {
        var first = UserProfileEntityKeys.BuildPartitionKey(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var second = UserProfileEntityKeys.BuildPartitionKey(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void BuildRowKey_UsesStableProfileValue()
    {
        Assert.Equal("profile", UserProfileEntityKeys.BuildRowKey());
    }
}
