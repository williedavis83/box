using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Auth.Contract;

public static class AuthProviderRegistrationExtensions
{
    public static IServiceCollection AddConfiguredAuthProvider(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<IAuthProviderRegistration> registrations)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(registrations);

        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()
            ?? new AuthOptions();

        if (string.IsNullOrWhiteSpace(authOptions.Provider))
        {
            throw new InvalidOperationException("Auth:Provider must be configured.");
        }

        var registration = registrations.FirstOrDefault(candidate =>
            string.Equals(candidate.ProviderName, authOptions.Provider, StringComparison.OrdinalIgnoreCase));

        if (registration is null)
        {
            throw new InvalidOperationException(
                $"No auth provider is registered for '{authOptions.Provider}'.");
        }

        registration.Register(services, configuration);
        return services;
    }
}
