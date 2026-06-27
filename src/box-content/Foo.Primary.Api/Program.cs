using BoxBottom.AzureTable.Emulation;
using Blabber.Emulator;
using BoxBottom.Emulation;
using Foo.Primary.Api.AzureTable;
using Foo.Primary.Api.Blabber;
using Foo.Primary.Api.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddBlabbers(builder.Configuration);
builder.Services.AddFooAzureTableScenarios(builder.Configuration);

builder.AddEmulation(typeof(BlabberController).Assembly, typeof(AzureTableBasicsController).Assembly)
    .RegisterBlabberEmulation()
    .RegisterAzureTableEmulation()
    .ApplyConfiguredEmulation();

var app = builder.Build();

app.MapOpenApi();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
