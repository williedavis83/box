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
    /// Adds the meta API project and wires catalog environment variables from <paramref name="apis"/>.
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddMetaProject(
        this IDistributedApplicationBuilder builder,
        MetaProjectOptions options,
        IReadOnlyDictionary<string, IResourceBuilder<ProjectResource>> apis,
        IResourceBuilder<IResource> web)
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

        var meta = builder.AddProject(options.StackName, options.ProjectPath, projectOptions =>
            {
                projectOptions.ExcludeLaunchProfile = true;
            })
            .WithHttpEndpoint(name: "http")
            .WithHttpHealthCheck("/health")
            .WithExternalHttpEndpoints()
            .WithUrlForEndpoint("http", url => url.Url = "/scalar")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment(StackNameEnvironmentVariable, options.StackPrefix)
            .WithEnvironment(HostingStartupAssembliesKey, string.Empty);

        foreach (var (key, value) in options.EnvironmentVariables)
        {
            meta = meta.WithEnvironment(key, value);
        }

        foreach (var (logicalName, apiResource) in apis)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(logicalName);
            ArgumentNullException.ThrowIfNull(apiResource);

            meta = meta.WithReference(apiResource).WaitFor(apiResource);
        }

        foreach (var (key, value) in MetaCatalogConfiguration.BuildApiCatalogEnvironmentVariables(apis, meta))
        {
            meta = meta.WithEnvironment(key, value);
        }

        return meta.WithParentRelationship(web.Resource);
    }

    /// <summary>
    /// Adds a YARP route on the edge proxy for the meta API at <c>/api/meta/{**catch-all}</c>.
    /// </summary>
    public static IResourceBuilder<ProjectResource> ConfigureMetaRoute(
        this IResourceBuilder<ProjectResource> edge,
        IResourceBuilder<ProjectResource> meta)
    {
        ArgumentNullException.ThrowIfNull(edge);
        ArgumentNullException.ThrowIfNull(meta);

        var routeKey = MetaCatalogConfiguration.MetaLogicalName;
        var clusterId = $"{routeKey}-cluster";

        edge.WithReference(meta)
            .WaitFor(meta)
            .WithEnvironment($"ReverseProxy__Routes__{routeKey}-route__ClusterId", clusterId)
            .WithEnvironment($"ReverseProxy__Routes__{routeKey}-route__Match__Path", EdgeRoutingConfiguration.BuildMetaRoutePath())
            .WithEnvironment(
                $"ReverseProxy__Routes__{routeKey}-route__Transforms__0__PathRemovePrefix",
                EdgeRoutingConfiguration.BuildMetaRoutePathPrefix())
            .WithEnvironment(
                $"ReverseProxy__Routes__{routeKey}-route__Transforms__1__PathPrefix",
                "/api");

        edge.WithEnvironment(
            $"ReverseProxy__Clusters__{clusterId}__Destinations__api__Address",
            EdgeRoutingConfiguration.BuildApiClusterAddress(meta.Resource.Name));

        return edge;
    }

    /// <summary>
    /// Adds a support-category project with standard HTTP and health-check configuration.
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddSupportProject(
        this IDistributedApplicationBuilder builder,
        string resourceName,
        string projectPath)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        return builder.AddProject(resourceName, projectPath, projectOptions =>
            {
                projectOptions.ExcludeLaunchProfile = true;
            })
            .WithHttpEndpoint(name: "http")
            .WithHttpHealthCheck("/health")
            .WithExternalHttpEndpoints()
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment(HostingStartupAssembliesKey, string.Empty);
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
