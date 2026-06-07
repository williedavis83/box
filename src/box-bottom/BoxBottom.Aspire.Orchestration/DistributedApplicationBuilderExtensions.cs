using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.JavaScript;
using CommunityToolkit.Aspire.Hosting.Dapr;

namespace BoxBottom.Aspire.Orchestration;

public static class DistributedApplicationBuilderExtensions
{
    private const string HostingStartupAssembliesKey = "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES";
    private const string StackNameEnvironmentVariable = "BOX_STACK_NAME";
    private const string DaprComponentsPath = "../../dapr/components";
    private const string DaprConfigPath = "../../dapr/config.yaml";

    /// <summary>
    /// Adds a .NET API project and applies standard Aspire configuration, including Scalar dashboard links.
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddApiProject(
        this IDistributedApplicationBuilder builder,
        ApiProjectOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.LogicalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectPath);

        var daprSidecarOptions = new DaprSidecarOptions
        {
            AppId = options.StackName,
            Config = DaprConfigPath,
            ResourcesPaths = [DaprComponentsPath],
            AppChannelAddress = options.GrpcOnlyAppChannel ? "127.0.0.1" : null,
            AppEndpoint = options.GrpcOnlyAppChannel ? "grpc" : null,
            AppProtocol = options.GrpcOnlyAppChannel ? "h2c" : null,
        };

        var api = builder.AddProject(options.StackName, options.ProjectPath, projectOptions =>
            {
                projectOptions.ExcludeLaunchProfile = true;
            });

        if (!options.GrpcOnlyAppChannel)
        {
            api = api.WithHttpEndpoint(name: "http");
        }

        api = api
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", options.AspNetEnvironment)
            .WithEnvironment(StackNameEnvironmentVariable, options.StackPrefix)
            .WithEnvironment(HostingStartupAssembliesKey, string.Empty);

        foreach (var (key, value) in options.EnvironmentVariables)
        {
            api = api.WithEnvironment(key, value);
        }

        api = api
            .WithExternalHttpEndpoints()
            .WithUrlForEndpoint("http", url => url.Url = "/scalar")
            .WithDaprSidecar(sidecar => sidecar.WithOptions(daprSidecarOptions))
            .WithHttpHealthCheck("/health");

        return api;
    }

    /// <summary>
    /// Adds a Vite web app and applies standard Aspire configuration.
    /// </summary>
    public static IResourceBuilder<ViteAppResource> AddWebProject(
        this IDistributedApplicationBuilder builder,
        WebProjectOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.LogicalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectPath);

        var web = builder.AddViteApp(options.StackName, options.ProjectPath)
            .WithPnpm()
            .WithExternalHttpEndpoints()
            .WithEnvironment(StackNameEnvironmentVariable, options.StackPrefix);

        foreach (var (key, value) in options.EnvironmentVariables)
        {
            web = web.WithEnvironment(key, value);
        }

        return web;
    }

    /// <summary>
    /// Adds the YARP edge reverse proxy, wires API references and routes from <paramref name="apis"/>,
    /// and configures dashboard parent relationships (edge under web, APIs under edge).
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddEdgeProject<TWeb>(
        this IDistributedApplicationBuilder builder,
        EdgeProjectOptions options,
        IReadOnlyDictionary<string, IResourceBuilder<ProjectResource>> apis,
        IResourceBuilder<TWeb> web)
        where TWeb : IResource
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.LogicalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectPath);
        ArgumentNullException.ThrowIfNull(apis);
        ArgumentNullException.ThrowIfNull(web);

        if (apis.Count == 0)
        {
            throw new ArgumentException("At least one API resource is required.", nameof(apis));
        }

        var edge = builder.AddProject(options.StackName, options.ProjectPath, projectOptions =>
            {
                projectOptions.ExcludeLaunchProfile = true;
            })
            .WithHttpEndpoint(name: "http")
            .WithHttpHealthCheck("/health")
            .WithExternalHttpEndpoints()
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment(StackNameEnvironmentVariable, options.StackPrefix)
            .WithEnvironment(HostingStartupAssembliesKey, string.Empty);

        foreach (var (key, value) in options.EnvironmentVariables)
        {
            edge = edge.WithEnvironment(key, value);
        }

        foreach (var (logicalName, apiResource) in apis.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase))
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(logicalName);
            ArgumentNullException.ThrowIfNull(apiResource);

            apiResource.WithParentRelationship(edge);
            edge.WithReference(apiResource).WaitFor(apiResource);

            var routeKey = EdgeRoutingConfiguration.BuildRouteKey(logicalName);
            var clusterId = $"{routeKey}-cluster";
            var routePath = EdgeRoutingConfiguration.BuildRoutePath(logicalName, apis.Count);

            edge.WithEnvironment($"ReverseProxy__Routes__{routeKey}-route__ClusterId", clusterId);
            edge.WithEnvironment($"ReverseProxy__Routes__{routeKey}-route__Match__Path", routePath);

            if (apis.Count > 1 && !EdgeRoutingConfiguration.IsDefaultApiRoute(logicalName))
            {
                var routePathPrefix = EdgeRoutingConfiguration.BuildRoutePathPrefix(logicalName);
                edge.WithEnvironment(
                    $"ReverseProxy__Routes__{routeKey}-route__Transforms__0__PathRemovePrefix",
                    routePathPrefix);
                edge.WithEnvironment(
                    $"ReverseProxy__Routes__{routeKey}-route__Transforms__1__PathPrefix",
                    "/api");
            }

            edge.WithEnvironment(
                $"ReverseProxy__Clusters__{clusterId}__Destinations__api__Address",
                EdgeRoutingConfiguration.BuildApiClusterAddress(apiResource.Resource.Name));
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

}
