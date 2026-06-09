namespace BoxTop.Meta.Api.Options;

public sealed class MetaCatalogOptions
{
    public const string SectionName = "Meta";

    public List<MetaApiEntryOptions> Apis { get; init; } = [];
}

public sealed class MetaApiEntryOptions
{
    public required string LogicalName { get; init; }

    public required string Url { get; init; }
}
