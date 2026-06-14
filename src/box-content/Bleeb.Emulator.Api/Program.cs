using Bleeb.Emulator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddSingleton<BleebAccountStore>();

var app = builder.Build();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
