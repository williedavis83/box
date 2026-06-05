using BoxBottom.Aspire.Orchestration;

var builder = DistributedApplication.CreateBuilder(args);

var primaryApi = builder.AddApiProject(
    name: "primary-api",
    projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj");

builder.AddViteApp("web", "../BoxTop.Web")
    .WithPnpm()
    .WithExternalHttpEndpoints()
    .WithReference(primaryApi)
    .WaitFor(primaryApi);

builder.Build().Run();
