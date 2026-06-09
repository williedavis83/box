using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace BoxBottom.Aspire.Orchestration;

public static class MetaCatalogConfiguration
{
    public const string MetaLogicalName = "meta";

    public const string OpenApiDocumentPath = "/openapi/v1.json";

    public static IEnumerable<KeyValuePair<string, string>> BuildApiCatalogEnvironmentVariables(
        IEnumerable<(string LogicalName, string AspireResourceName)> apis,
        string metaAspireResourceName)
    {
        ArgumentNullException.ThrowIfNull(apis);
        ArgumentException.ThrowIfNullOrWhiteSpace(metaAspireResourceName);

        var index = 0;

        foreach (var (logicalName, aspireResourceName) in apis)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(logicalName);
            ArgumentException.ThrowIfNullOrWhiteSpace(aspireResourceName);

            yield return new KeyValuePair<string, string>(
                BuildCatalogKey(index, "LogicalName"),
                logicalName);
            yield return new KeyValuePair<string, string>(
                BuildCatalogKey(index, "Url"),
                EdgeRoutingConfiguration.BuildApiClusterAddress(aspireResourceName));
            index++;
        }

        yield return new KeyValuePair<string, string>(
            BuildCatalogKey(index, "LogicalName"),
            MetaLogicalName);
        yield return new KeyValuePair<string, string>(
            BuildCatalogKey(index, "Url"),
            EdgeRoutingConfiguration.BuildApiClusterAddress(metaAspireResourceName));
    }

    public static IEnumerable<KeyValuePair<string, string>> BuildApiCatalogEnvironmentVariables(
        IReadOnlyDictionary<string, IResourceBuilder<ProjectResource>> apis,
        IResourceBuilder<ProjectResource> meta)
    {
        ArgumentNullException.ThrowIfNull(apis);
        ArgumentNullException.ThrowIfNull(meta);

        var catalogEntries = apis
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .Select(entry => (entry.Key, entry.Value.Resource.Name));

        return BuildApiCatalogEnvironmentVariables(catalogEntries, meta.Resource.Name);
    }

    private static string BuildCatalogKey(int index, string propertyName) =>
        $"Meta__Apis__{index}__{propertyName}";
}
