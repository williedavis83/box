using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.JavaScript;

namespace BoxBottom.Aspire.Orchestration;

public sealed class StackOperations(IDistributedApplicationBuilder builder,
    string webProjectPath, 
    string edgeProjectPath,
    string metaProjectPath)
{
    public const string ToolsCategoryName = "tools";
    private const string WebLogicalName = "web";
    private const string EdgeLogicalName = "edge";

    private readonly IDistributedApplicationBuilder _builder = builder;
    private IResourceBuilder<CategoryResource>? _toolsCategory;

    public StackDefinition CreateStackDefinition(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var stack = new StackDefinition(name,
            web: new WebProjectOptions(
                logicalName: "web",
                projectPath: webProjectPath),
            edge: new EdgeProjectOptions(
                logicalName: "edge",
                projectPath: edgeProjectPath),
            meta: new MetaProjectOptions(
                logicalName: MetaCatalogConfiguration.MetaLogicalName,
                projectPath: metaProjectPath)
        );

        return stack;
    }

    public IResourceBuilder<TResource> OrchestrateTool<TResource>(
        Func<IDistributedApplicationBuilder, IResourceBuilder<TResource>> configureTool)
        where TResource : IResource
    {
        ArgumentNullException.ThrowIfNull(configureTool);

        var tools = _toolsCategory ??= _builder.AddCategory(ToolsCategoryName);
        return configureTool(_builder).WithParentRelationship(tools.Resource);
    }

    public IResourceBuilder<JavaScriptAppResource> OrchestratePlaywrightTool(
        IReadOnlyDictionary<string, StackResources> stacks,
        string playwrightProjectPath,
        string resourceName = "playwright-ui",
        string runScriptName = "test")
    {
        ArgumentNullException.ThrowIfNull(stacks);
        ArgumentException.ThrowIfNullOrWhiteSpace(playwrightProjectPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(runScriptName);

        return OrchestrateTool(appBuilder =>
        {
            var playwright = appBuilder.AddJavaScriptApp(resourceName, playwrightProjectPath, runScriptName: runScriptName)
                .WithPnpm()
                .WithExplicitStart();

            foreach (var (stackName, stack) in stacks)
            {
                playwright = playwright
                    .WaitFor(stack.Web)
                    .WaitFor(stack.Edge)
                    .WaitFor(stack.Meta);

                playwright = AddPlaywrightHttpEndpoint(
                    playwright,
                    stackName,
                    WebLogicalName,
                    stack.Web.GetEndpoint("http"));
                playwright = AddPlaywrightHttpEndpoint(
                    playwright,
                    stackName,
                    EdgeLogicalName,
                    stack.Edge.GetEndpoint("http"));
                playwright = AddPlaywrightHttpEndpoint(
                    playwright,
                    stackName,
                    MetaCatalogConfiguration.MetaLogicalName,
                    stack.Meta.GetEndpoint("http"));

                foreach (var (apiLogicalName, api) in stack.Apis)
                {
                    playwright = playwright.WaitFor(api);
                    playwright = AddPlaywrightHttpEndpoint(
                        playwright,
                        stackName,
                        apiLogicalName,
                        api.GetEndpoint("http"));
                }
            }

            return playwright;
        });
    }

    public static string BuildHttpEnvironmentVariable(string stackName, string logicalName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stackName);
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalName);

        return $"{NormalizeEnvironmentKey(stackName)}_{NormalizeEnvironmentKey(logicalName)}_HTTP";
    }

    private static string NormalizeEnvironmentKey(string value) =>
        value.Trim().ToUpperInvariant().Replace('-', '_');

    private static IResourceBuilder<JavaScriptAppResource> AddPlaywrightHttpEndpoint(
        IResourceBuilder<JavaScriptAppResource> playwright,
        string stackName,
        string logicalName,
        EndpointReference endpoint) =>
        playwright
            .WithReference(endpoint)
            .WithEnvironment(
                BuildHttpEnvironmentVariable(stackName, logicalName),
                endpoint.Property(EndpointProperty.Url));

    public StackResources OrchestrateStack(StackDefinition stack)
    {
        ArgumentNullException.ThrowIfNull(stack);

        var apis = stack.GetApis().ToDictionary(
            api => api.LogicalName,
            api => _builder.AddApiProject(api),
            StringComparer.OrdinalIgnoreCase);

        if (apis.Count == 0)
        {
            throw new InvalidOperationException("Stack must contain at least one API before orchestration.");
        }

        var web = _builder.AddWebProject(stack.Web);

        var edge = _builder.AddEdgeProject(
            stack.Edge,
            apis,
            web);

        var meta = _builder.AddMetaProject(
            stack.Meta,
            apis,
            web);

        edge.ConfigureMetaRoute(meta);

        var webHttpEndpoint = web.GetEndpoint("http");
        var edgeHttpEndpoint = edge.GetEndpoint("http");
        var metaHttpEndpoint = meta.GetEndpoint("http");

        web.WithReference(edge)
            .WaitFor(edge)
            .WithEnvironment("EDGE_HTTP", edgeHttpEndpoint.Property(EndpointProperty.Url))
            .WithEnvironment(
                BuildHttpEnvironmentVariable(stack.Name, WebLogicalName),
                webHttpEndpoint.Property(EndpointProperty.Url))
            .WithEnvironment(
                BuildHttpEnvironmentVariable(stack.Name, stack.Edge.LogicalName),
                edgeHttpEndpoint.Property(EndpointProperty.Url));

        meta.WithEnvironment(
            BuildHttpEnvironmentVariable(stack.Name, MetaCatalogConfiguration.MetaLogicalName),
            metaHttpEndpoint.Property(EndpointProperty.Url));

        return new StackResources
        {
            Web = web,
            Edge = edge,
            Meta = meta,
            Apis = apis,
        };
    }

}
