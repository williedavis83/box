using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Contract.Models;
using BoxBottom.Users.Contract.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BoxBottom.Auth.Entra;

public static class EntraAuthServiceCollectionExtensions
{
    public static IServiceCollection AddEntraAuth<TProfile>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TProfile : BaseUserProfile
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<EntraAuthOptions>(configuration.GetSection(EntraAuthOptions.SectionName));
        services.AddSingleton<IEntraClaimsMapper, EntraClaimsMapper>();

        var entraOptions = configuration.GetSection(EntraAuthOptions.SectionName).Get<EntraAuthOptions>()
            ?? new EntraAuthOptions();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "box.auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Path = "/";
                options.SlidingExpiration = true;
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = string.IsNullOrWhiteSpace(entraOptions.Authority)
                    ? $"https://login.microsoftonline.com/{entraOptions.TenantId}/v2.0"
                    : entraOptions.Authority.TrimEnd('/');
                options.RequireHttpsMetadata = options.Authority.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
                options.ClientId = entraOptions.ClientId;
                options.ClientSecret = entraOptions.ClientSecret;
                options.CallbackPath = entraOptions.CallbackPath;
                options.SignedOutCallbackPath = entraOptions.SignedOutCallbackPath;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = false;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.CorrelationCookie.Path = "/";
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.NonceCookie.Path = "/";
                options.NonceCookie.SameSite = SameSiteMode.Lax;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        var origin = !string.IsNullOrWhiteSpace(entraOptions.PublicOrigin)
                            ? entraOptions.PublicOrigin.TrimEnd('/')
                            : $"{context.Request.Scheme}://{context.Request.Host}";

                        context.ProtocolMessage.RedirectUri =
                            $"{origin}{entraOptions.ExternalCallbackPath}";

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var authService = context.HttpContext.RequestServices
                            .GetRequiredService<IAuthService<TProfile>>();
                        var mapper = context.HttpContext.RequestServices
                            .GetRequiredService<IEntraClaimsMapper>();

                        var (externalUser, profileSeed) = mapper.Map(context.Principal!);
                        await authService.SignInAsync(externalUser, profileSeed, context.HttpContext.RequestAborted);

                        var returnPath = context.Properties?.RedirectUri ?? "/";
                        var returnUrl = returnPath.StartsWith('/')
                            && !string.IsNullOrWhiteSpace(entraOptions.PublicOrigin)
                            ? $"{entraOptions.PublicOrigin.TrimEnd('/')}{returnPath}"
                            : returnPath;

                        context.HandleResponse();
                        context.Response.Redirect(returnUrl);
                    },
                };
            });

        services.AddAuthorization();

        return services;
    }
}
