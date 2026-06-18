using BoxBottom.Azure.Table;
using BoxBottom.Azure.Table.Models;
using BoxBottom.Azure.Table.Options;
using BoxBottom.Azure.Table.Services;
using Foo.Primary.Shared.AzureTable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class AzureTableBootstrapHostedServiceTests
{
    [Fact]
    public async Task StartAsync_CreatesConfiguredTables_WhenStorageConfigured()
    {
        var recordingService = new RecordingAzureTableService(isConfigured: true);
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IAzureTableService>(AzureTableKeys.Orders, recordingService);
        services.AddSingleton<IOptions<AzureTableBootstrapOptions>>(
            Options.Create(new AzureTableBootstrapOptions
            {
                Entries =
                [
                    new AzureTableBootstrapEntry
                    {
                        AnchorKey = AzureTableKeys.Orders,
                        Tables = ["orders", "demo"],
                    },
                ],
            }));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new AzureTableBootstrapHostedService(
            provider,
            provider.GetRequiredService<IOptions<AzureTableBootstrapOptions>>(),
            NullLogger<AzureTableBootstrapHostedService>.Instance);

        await hostedService.StartAsync(CancellationToken.None);

        Assert.Equal(["orders", "demo"], recordingService.CreatedTables);
    }

    [Fact]
    public async Task StartAsync_Skips_WhenAnchorKeyMissing()
    {
        var recordingService = new RecordingAzureTableService(isConfigured: true);
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IAzureTableService>(AzureTableKeys.Orders, recordingService);
        services.AddSingleton<IOptions<AzureTableBootstrapOptions>>(
            Options.Create(new AzureTableBootstrapOptions
            {
                Entries =
                [
                    new AzureTableBootstrapEntry
                    {
                        Tables = ["orders"],
                    },
                ],
            }));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new AzureTableBootstrapHostedService(
            provider,
            provider.GetRequiredService<IOptions<AzureTableBootstrapOptions>>(),
            NullLogger<AzureTableBootstrapHostedService>.Instance);

        await hostedService.StartAsync(CancellationToken.None);

        Assert.Empty(recordingService.CreatedTables);
    }

    [Fact]
    public async Task StartAsync_Skips_WhenStorageUnconfigured()
    {
        var recordingService = new RecordingAzureTableService(isConfigured: false);
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IAzureTableService>(AzureTableKeys.Orders, recordingService);
        services.AddSingleton<IOptions<AzureTableBootstrapOptions>>(
            Options.Create(new AzureTableBootstrapOptions
            {
                Entries =
                [
                    new AzureTableBootstrapEntry
                    {
                        AnchorKey = AzureTableKeys.Orders,
                        Tables = ["orders"],
                    },
                ],
            }));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new AzureTableBootstrapHostedService(
            provider,
            provider.GetRequiredService<IOptions<AzureTableBootstrapOptions>>(),
            NullLogger<AzureTableBootstrapHostedService>.Instance);

        await hostedService.StartAsync(CancellationToken.None);

        Assert.Empty(recordingService.CreatedTables);
    }

    private sealed class RecordingAzureTableService(bool isConfigured) : IAzureTableService
    {
        public List<string> CreatedTables { get; } = [];

        public AzureTableConfigurationStatus GetConfigurationStatus() =>
            new(isConfigured, isConfigured ? "ConnectionString" : "None");

        public Task<IReadOnlyList<string>> ListTablesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);

        public async Task CreateTableIfNotExistsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            CreatedTables.Add(tableName);
            await Task.CompletedTask;
        }

        public Task<AzureTableEntityResult?> GetEntityAsync(
            string tableName,
            string partitionKey,
            string rowKey,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<AzureTableEntityResult?>(null);

        public Task<IReadOnlyList<AzureTableEntityResult>> QueryEntitiesAsync(
            string tableName,
            int? maxResults = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AzureTableEntityResult>>([]);

        public Task UpsertEntityAsync(
            string tableName,
            AzureTableUpsertEntityRequest entity,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
