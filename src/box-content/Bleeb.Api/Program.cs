using Bleeb.Api.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.Configure<BleebAccountsOptions>(
    builder.Configuration.GetSection(BleebAccountsOptions.SectionName));

var app = builder.Build();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
