using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Contract.Models;
using Microsoft.Extensions.Options;

namespace BoxBottom.Auth.ZeroAuth;

public sealed class ZeroAuthRequestValidator(IOptions<ZeroAuthOptions> options) : IZeroAuthRequestValidator
{
    public string? Validate(ZeroAuthLoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Provider))
        {
            return "Provider is required.";
        }

        if (string.IsNullOrWhiteSpace(request.ExternalId))
        {
            return "ExternalId is required.";
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return "DisplayName is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return "Email is required.";
        }

        var allowedProviders = options.Value.AllowedProviders ?? [];
        if (allowedProviders.Length > 0
            && !allowedProviders.Contains(request.Provider, StringComparer.OrdinalIgnoreCase))
        {
            return $"Provider '{request.Provider}' is not allowed.";
        }

        return null;
    }
}
