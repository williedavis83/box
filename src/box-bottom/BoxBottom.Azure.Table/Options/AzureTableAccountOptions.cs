namespace BoxBottom.Azure.Table.Options;

public sealed class AzureTableAccountOptions
{
    public string? ConnectionString { get; init; }

    public Uri? ServiceUri { get; init; }
}
