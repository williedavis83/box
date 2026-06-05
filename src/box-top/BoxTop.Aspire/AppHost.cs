using BoxBottom.Aspire.Orchestration;

var builder = DistributedApplication.CreateBuilder(args);

var primaryApi = builder.AddApiProject(
    name: "primary-api",
    projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj");

var apis = new Dictionary<string, IResourceBuilder<ProjectResource>>
{
    ["primary-api"] = primaryApi,
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

builder.Build().Run();
