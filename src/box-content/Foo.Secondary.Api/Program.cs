using Foo.Secondary.Api.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<WorldOptions>(builder.Configuration.GetSection(WorldOptions.SectionName));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
