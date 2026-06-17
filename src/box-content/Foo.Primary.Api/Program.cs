using Blabber.Emulator;
using BoxBottom.Azure.Table;
using BoxBottom.Emulation;
using Foo.Primary.Api.Blabber;
using Foo.Primary.Api.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddBlabbers(builder.Configuration);
builder.Services.AddAzureTableStorage(builder.Configuration);

builder.AddEmulation(typeof(BlabberController).Assembly)
    .RegisterBlabberEmulation()
    .ApplyConfiguredEmulation();

var app = builder.Build();

app.MapOpenApi();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
