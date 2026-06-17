namespace BoxBottom.Azure.Table.Models;

public sealed record AzureTableEntityResult(
    string PartitionKey,
    string RowKey,
    DateTimeOffset? Timestamp,
    IReadOnlyDictionary<string, object?> Properties);
