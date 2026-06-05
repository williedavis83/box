var builder = DistributedApplication.CreateBuilder(args);

var primaryApi = builder.AddProject<Projects.Foo_Primary_Api>("primary-api")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.AddViteApp("web", "../BoxTop.Web")
    .WithPnpm()
    .WithExternalHttpEndpoints()
    .WithReference(primaryApi)
    .WaitFor(primaryApi);

builder.Build().Run();
