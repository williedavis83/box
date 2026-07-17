using BoxBottom.AzureTable.Emulation;
using BoxBottom.Auth.Emulation;
using BoxBottom.Emulation;
using BoxPack.Auth.Customization;
using BoxPack.Users.Customization;
using BoxPack.Users.Customization.Controllers;
using BoxTop.Users.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddBoxKeyVaultConfiguration();
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddBoxPackUsers(builder.Configuration);

builder.AddEmulation(typeof(UserProfileController).Assembly)
    .RegisterEntraEmulation()
    .RegisterAzureTableEmulation()
    .ApplyConfiguredEmulation();

// Auth emulation is applied after Key Vault and before auth registration so the
// transmitted Keycloak settings win without bypassing the normal configuration path.
builder.Services.AddBoxPackAuth(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
