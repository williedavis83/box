namespace BoxBottom.Azure.Table.Options;

public sealed class AzureTableBootstrapEntry
{
    public string? AnchorKey { get; init; }

    public IReadOnlyList<string> Tables { get; init; } = [];
}
