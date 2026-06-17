using Azure.Data.Tables;

namespace BoxBottom.Azure.Table.Internal;

internal interface IAzureTableServiceClientFactory
{
    AzureTableConfigurationResolution Resolve();

    TableServiceClient CreateClient();
}

internal sealed record AzureTableConfigurationResolution(bool IsConfigured, string Mode);
