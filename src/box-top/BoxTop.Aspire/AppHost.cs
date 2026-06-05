var builder = DistributedApplication.CreateBuilder(args);

builder.AddViteApp("web", "../BoxTop.Web")
    .WithPnpm()
    .WithExternalHttpEndpoints();

builder.Build().Run();
