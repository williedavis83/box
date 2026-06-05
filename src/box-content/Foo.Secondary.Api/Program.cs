using Foo.Secondary.Api.Options;
using Foo.Secondary.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<WorldOptions>(builder.Configuration.GetSection(WorldOptions.SectionName));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGrpcService<WorldGrpcService>();
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
