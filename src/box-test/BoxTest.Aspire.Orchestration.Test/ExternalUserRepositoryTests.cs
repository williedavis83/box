using Azure.Data.Tables;
using BoxBottom.Azure.Table;
using BoxBottom.Azure.Table.Models;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Entities;
using BoxBottom.Users.Contract.Models;
using BoxBottom.Users.Data;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class ExternalUserRepositoryTests
{
    [Fact]
    public async Task GetOrCreateInternalUserIdAsync_CreatesAssociation_WhenExternalUserNotMapped()
    {
        var tableService = new InMemoryAzureTableService();
        var mapper = new TestExternalUserMapper();
        var repository = new AzureTableExternalUserRepository<TestAuthUser>(
            tableService,
            mapper);

        var internalUserId = await repository.GetOrCreateInternalUserIdAsync(
            new TestAuthUser("AzureAd", "oid-123"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, internalUserId);
        Assert.Single(tableService.UpsertedEntities);
        Assert.Equal(1, mapper.MapInvocationCount);

        var entity = Assert.IsType<ExternalUserTableEntity>(tableService.UpsertedEntities[0]);
        Assert.Equal("azuread", entity.PartitionKey);
        Assert.Equal("oid-123", entity.RowKey);
        Assert.Equal(internalUserId, entity.InternalUserId);
    }

    [Fact]
    public async Task GetOrCreateInternalUserIdAsync_ReturnsExistingInternalUserId_WhenAssociationExists()
    {
        var existingInternalUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var tableService = new InMemoryAzureTableService();
        tableService.SeedEntity(
            ExternalUserEntityKeys.TableName,
            new ExternalUserTableEntity
            {
                PartitionKey = "azuread",
                RowKey = "oid-123",
                InternalUserId = existingInternalUserId,
                AssociatedAtUtc = DateTimeOffset.UtcNow,
            });

        var mapper = new TestExternalUserMapper();
        var repository = new AzureTableExternalUserRepository<TestAuthUser>(
            tableService,
            mapper);

        var internalUserId = await repository.GetOrCreateInternalUserIdAsync(
            new TestAuthUser("AzureAd", "oid-123"),
            CancellationToken.None);

        Assert.Equal(existingInternalUserId, internalUserId);
        Assert.Empty(tableService.UpsertedEntities);
        Assert.Equal(1, mapper.MapInvocationCount);
    }

    [Fact]
    public async Task GetOrCreateInternalUserIdAsync_NormalizesProviderAndExternalIdKeys()
    {
        var tableService = new InMemoryAzureTableService();
        var repository = new AzureTableExternalUserRepository<TestAuthUser>(
            tableService,
            new TestExternalUserMapper());

        await repository.GetOrCreateInternalUserIdAsync(
            new TestAuthUser("  AzureAd  ", "  oid-456  "),
            CancellationToken.None);

        var entity = Assert.IsType<ExternalUserTableEntity>(tableService.UpsertedEntities[0]);
        Assert.Equal("azuread", entity.PartitionKey);
        Assert.Equal("oid-456", entity.RowKey);
    }

    [Fact]
    public async Task GetOrCreateInternalUserIdAsync_Throws_WhenExternalUserIsNull()
    {
        var repository = new AzureTableExternalUserRepository<TestAuthUser>(
            new InMemoryAzureTableService(),
            new TestExternalUserMapper());

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repository.GetOrCreateInternalUserIdAsync(null!, CancellationToken.None));
    }

    private sealed record TestAuthUser(string Provider, string ExternalId);

    private sealed class TestExternalUserMapper : IExternalUserMapper<TestAuthUser>
    {
        public int MapInvocationCount { get; private set; }

        public ExternalUser Map(TestAuthUser externalUser)
        {
            ArgumentNullException.ThrowIfNull(externalUser);
            MapInvocationCount++;
            return new ExternalUser(externalUser.Provider, externalUser.ExternalId);
        }
    }
}
