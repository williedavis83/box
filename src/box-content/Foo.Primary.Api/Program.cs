using Blabber.Emulator;
using BoxBottom.Emulation;
using Foo.Primary.Api.Blabber;
using Foo.Primary.Api.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddBlabbers(builder.Configuration);
builder.RegisterBlabberEmulation();
builder.EnableEmulationSupport(typeof(BlabberController).Assembly);
builder.ApplyConfiguredEmulation();

var app = builder.Build();

app.MapOpenApi();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
