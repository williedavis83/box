using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.Dapr;

namespace BoxBottom.Aspire.Orchestration;

public static class DistributedApplicationBuilderExtensions
{
    private const string HostingStartupAssembliesKey = "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES";
    private const string DaprComponentsPath = "../../dapr/components";
    private const string DaprConfigPath = "../../dapr/config.yaml";

    /// <summary>
    /// Adds a .NET API project and applies standard Aspire configuration, including Scalar dashboard links.
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddApiProject(
        this IDistributedApplicationBuilder builder,
        string name,
        string projectPath,
        string aspNetEnvironment = "Development",
        bool grpcOnlyAppChannel = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var daprSidecarOptions = new DaprSidecarOptions
        {
            AppId = name,
            Config = DaprConfigPath,
            ResourcesPaths = [DaprComponentsPath],
            AppChannelAddress = grpcOnlyAppChannel ? "127.0.0.1" : null,
            AppEndpoint = grpcOnlyAppChannel ? "http" : null,
            AppProtocol = grpcOnlyAppChannel ? "h2c" : null,
        };

        var api = builder.AddProject(name, projectPath, options =>
            {
                options.ExcludeLaunchProfile = true;
            })
            .WithHttpEndpoint(name: "http");

        if (grpcOnlyAppChannel)
        {
            api = api.WithEnvironment("GRPC_ONLY_APP_CHANNEL", "true");
        }

        api = api
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspNetEnvironment)
            .WithEnvironment(HostingStartupAssembliesKey, string.Empty)
            .WithExternalHttpEndpoints()
            .WithUrlForEndpoint("https", url => url.Url = "/scalar")
            .WithUrlForEndpoint("http", url => url.Url = "/scalar")
            .WithDaprSidecar(sidecar => sidecar.WithOptions(daprSidecarOptions));

        if (!grpcOnlyAppChannel)
        {
            api = api.WithHttpHealthCheck("/health");
        }

        return api;
    }

    /// <summary>
    /// Adds the YARP edge reverse proxy, wires API references and routes from <paramref name="apis"/>,
    /// and configures dashboard parent relationships (edge under web, APIs under edge).
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddEdgeProject<TWeb>(
        this IDistributedApplicationBuilder builder,
        string name,
        string projectPath,
        IReadOnlyDictionary<string, IResourceBuilder<ProjectResource>> apis,
        IResourceBuilder<TWeb> web)
        where TWeb : IResource
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);
        ArgumentNullException.ThrowIfNull(apis);
        ArgumentNullException.ThrowIfNull(web);

        if (apis.Count == 0)
        {
            throw new ArgumentException("At least one API resource is required.", nameof(apis));
        }

        var edge = builder.AddProject(name, projectPath)
            .WithHttpHealthCheck("/health")
            .WithExternalHttpEndpoints();

        foreach (var (logicalName, apiResource) in apis.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase))
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(logicalName);
            ArgumentNullException.ThrowIfNull(apiResource);

            apiResource.WithParentRelationship(edge);
            edge.WithReference(apiResource).WaitFor(apiResource);

            var routeKey = BuildRouteKey(logicalName);
            var clusterId = $"{routeKey}-cluster";
            var routePath = BuildRoutePath(logicalName, apis.Count);

            edge.WithEnvironment($"ReverseProxy__Routes__{routeKey}-route__ClusterId", clusterId);
            edge.WithEnvironment($"ReverseProxy__Routes__{routeKey}-route__Match__Path", routePath);

            if (apis.Count > 1 && !IsDefaultApiRoute(logicalName))
            {
                var routePathPrefix = BuildRoutePathPrefix(logicalName);
                edge.WithEnvironment(
                    $"ReverseProxy__Routes__{routeKey}-route__Transforms__0__PathRemovePrefix",
                    routePathPrefix);
                edge.WithEnvironment(
                    $"ReverseProxy__Routes__{routeKey}-route__Transforms__1__PathPrefix",
                    "/api");
            }

            edge.WithEnvironment(
                $"ReverseProxy__Clusters__{clusterId}__Destinations__api__Address",
                $"http://{apiResource.Resource.Name}");
        }

        return edge.WithParentRelationship(web.Resource);
    }

    /// <summary>
    /// Adds a category resource for dashboard grouping.
    /// </summary>
    public static IResourceBuilder<CategoryResource> AddCategory(
        this IDistributedApplicationBuilder builder,
        string name)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var resource = new CategoryResource(name);

        return builder.AddResource(resource)
            .WithInitialState(new CustomResourceSnapshot
            {
                ResourceType = "Category",
                Properties = [],
                State = new ResourceStateSnapshot(KnownResourceStates.Running, KnownResourceStateStyles.Success),
                CreationTimeStamp = DateTime.UtcNow,
            })
            .ExcludeFromManifest();
    }

    private static string BuildRoutePathPrefix(string logicalName)
    {
        var normalized = BuildRouteKey(logicalName);
        if (normalized.EndsWith("-api", StringComparison.Ordinal))
        {
            normalized = normalized[..^4];
        }

        return $"/api/{normalized}";
    }

    private static bool IsDefaultApiRoute(string logicalName) =>
        string.Equals(logicalName, "primary-api", StringComparison.OrdinalIgnoreCase);

    private static string BuildRoutePath(string logicalName, int apiCount)
    {
        if (apiCount == 1 || IsDefaultApiRoute(logicalName))
        {
            return "/api/{**catch-all}";
        }

        var normalized = BuildRouteKey(logicalName);
        if (normalized.EndsWith("-api", StringComparison.Ordinal))
        {
            normalized = normalized[..^4];
        }

        return $"/api/{normalized}/{{**catch-all}}";
    }

    private static string BuildRouteKey(string logicalName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalName);
        return logicalName.Trim().ToLowerInvariant().Replace('_', '-');
    }
}
