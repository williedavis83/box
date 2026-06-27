using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Contract.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Auth.ZeroAuth;

public static class ZeroAuthServiceCollectionExtensions
{
    public static IServiceCollection AddZeroAuth(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ZeroAuthOptions>(configuration.GetSection(ZeroAuthOptions.SectionName));
        services.AddSingleton<IZeroAuthRequestValidator, ZeroAuthRequestValidator>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "box.auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.SlidingExpiration = true;
            });

        services.AddAuthorization();

        return services;
    }
}
