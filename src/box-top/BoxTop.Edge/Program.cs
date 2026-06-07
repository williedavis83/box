using BoxBottom.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/api/StackName", (StackProperties stack) => Results.Json(new { stackName = stack.StackName }));
app.MapGet("/", () => "BoxTop.Edge");
app.MapReverseProxy();

app.Run();
