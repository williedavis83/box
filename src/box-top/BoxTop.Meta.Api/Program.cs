using BoxTop.Meta.Api.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<MetaCatalogOptions>(builder.Configuration.GetSection(MetaCatalogOptions.SectionName));

var app = builder.Build();

app.MapOpenApi();
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
