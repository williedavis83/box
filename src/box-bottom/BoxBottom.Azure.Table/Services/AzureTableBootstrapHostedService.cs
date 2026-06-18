using BoxBottom.Azure.Table.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BoxBottom.Azure.Table.Services;

public sealed class AzureTableBootstrapHostedService(
    IServiceProvider serviceProvider,
    IOptions<AzureTableBootstrapOptions> options,
    ILogger<AzureTableBootstrapHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var entry in options.Value.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.AnchorKey))
            {
                logger.LogWarning("Skipping Azure Table bootstrap entry with no anchor key.");
                continue;
            }

            var azureTableService = serviceProvider.GetRequiredKeyedService<IAzureTableService>(entry.AnchorKey);
            var status = azureTableService.GetConfigurationStatus();
            if (!status.IsConfigured)
            {
                logger.LogInformation(
                    "Azure Table bootstrap skipped for anchor '{AnchorKey}' because storage is not configured.",
                    entry.AnchorKey);
                continue;
            }

            if (entry.Tables.Count == 0)
            {
                logger.LogInformation(
                    "Azure Table bootstrap skipped for anchor '{AnchorKey}' because no tables are configured.",
                    entry.AnchorKey);
                continue;
            }

            foreach (var tableName in entry.Tables)
            {
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    logger.LogWarning(
                        "Skipping empty Azure Table bootstrap entry for anchor '{AnchorKey}'.",
                        entry.AnchorKey);
                    continue;
                }

                await azureTableService.CreateTableIfNotExistsAsync(tableName, cancellationToken);
                logger.LogInformation(
                    "Ensured Azure Table '{TableName}' for anchor '{AnchorKey}'.",
                    tableName,
                    entry.AnchorKey);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
