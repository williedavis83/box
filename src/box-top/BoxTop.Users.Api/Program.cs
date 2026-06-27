using BoxBottom.AzureTable.Emulation;
using BoxBottom.Emulation;
using BoxPack.Auth.Customization;
using BoxPack.Users.Customization;
using BoxPack.Users.Customization.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddBoxPackUsers(builder.Configuration);
builder.Services.AddBoxPackAuth(builder.Configuration);

builder.AddEmulation(typeof(UserProfileController).Assembly)
    .RegisterAzureTableEmulation()
    .ApplyConfiguredEmulation();

var app = builder.Build();

app.MapOpenApi();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
