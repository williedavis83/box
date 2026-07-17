using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.JavaScript;

namespace BoxBottom.Aspire.Orchestration;

public sealed class StackResources
{
    public required IResourceBuilder<ViteAppResource> Web { get; init; }

    public required IResourceBuilder<ProjectResource> Edge { get; init; }

    public required IResourceBuilder<ProjectResource> Meta { get; init; }

    public required IReadOnlyDictionary<string, IResourceBuilder<ProjectResource>> Apis { get; init; }

    public bool IsIntegrationStack { get; set; }

    public string StackName { get; init; } = string.Empty;
}
