namespace BoxBottom.Azure.Table.Options;

public sealed class AzureTableBootstrapOptions
{
    public const string SectionName = "AzureTableBootstrap";

    public IReadOnlyList<AzureTableBootstrapEntry> Entries { get; init; } = [];
}
