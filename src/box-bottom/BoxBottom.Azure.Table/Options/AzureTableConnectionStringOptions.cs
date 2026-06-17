namespace BoxBottom.Azure.Table.Options;

public sealed class AzureTableConnectionStringOptions
{
    public const string SectionName = "AzureTableConnectionString";

    public string? ConnectionString { get; init; }
}
