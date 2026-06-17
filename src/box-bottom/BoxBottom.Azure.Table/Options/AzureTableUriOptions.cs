namespace BoxBottom.Azure.Table.Options;

public sealed class AzureTableUriOptions
{
    public const string SectionName = "AzureTableUri";

    public Uri? ServiceUri { get; init; }
}
