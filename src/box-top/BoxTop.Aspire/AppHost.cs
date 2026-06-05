using BoxBottom.Aspire.Orchestration;

var builder = DistributedApplication.CreateBuilder(args);

var primaryApi = builder.AddApiProject(
    name: "primary-api",
    projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj");

var secondaryApi = builder.AddApiProject(
    name: "secondary-api",
    projectPath: @"..\..\box-content\Foo.Secondary.Api\Foo.Secondary.Api.csproj",
    grpcOnlyAppChannel: true);

var apis = new Dictionary<string, IResourceBuilder<ProjectResource>>
{
    ["primary-api"] = primaryApi,
    ["secondary-api"] = secondaryApi,
};

var web = builder.AddViteApp("web", "../BoxTop.Web")
    .WithPnpm()
    .WithExternalHttpEndpoints();

var edge = builder.AddEdgeProject(
    name: "edge",
    projectPath: @"..\BoxTop.Edge\BoxTop.Edge.csproj",
    apis: apis,
    web: web);

web.WithReference(edge)
    .WaitFor(edge)
    .WithEnvironment("BOX_EDGE_HTTP", edge.GetEndpoint("http"));

var tools = builder.AddCategory("tools");

builder.AddJavaScriptApp("playwright-ui", @"..\..\box-test\BoxTest.Playwright", runScriptName: "test")
    .WithPnpm()
    .WithExplicitStart()
    .WithParentRelationship(tools.Resource)
    .WithReference(web.GetEndpoint("http"))
    .WithReference(edge.GetEndpoint("http"))
    .WaitFor(web)
    .WaitFor(edge)
    .WithEnvironment("BOX_WEB_HTTP", web.GetEndpoint("http"))
    .WithEnvironment("BOX_EDGE_HTTP", edge.GetEndpoint("http"));

builder.Build().Run();
