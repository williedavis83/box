namespace BoxBottom.Azure.Table.Models;

public sealed class AzureTableUpsertEntityRequest
{
    public string PartitionKey { get; init; } = string.Empty;

    public string RowKey { get; init; } = string.Empty;

    public Dictionary<string, object?> Properties { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
