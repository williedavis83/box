using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace BoxBottom.Aspire.Orchestration;

public static class DistributedApplicationBuilderExtensions
{
    /// <summary>
    /// Adds a .NET API project and applies standard Aspire configuration, including Scalar dashboard links.
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddApiProject(
        this IDistributedApplicationBuilder builder,
        string name,
        string projectPath)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        return builder.AddProject(name, projectPath)
            .WithHttpHealthCheck("/health")
            .WithExternalHttpEndpoints()
            .WithUrlForEndpoint("https", url => url.Url = "/scalar")
            .WithUrlForEndpoint("http", url => url.Url = "/scalar");
    }
}
