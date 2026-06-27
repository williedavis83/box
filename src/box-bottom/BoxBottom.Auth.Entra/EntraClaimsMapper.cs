using System.Security.Claims;
using BoxBottom.Auth.Contract.Models;
using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Auth.Entra;

public sealed class EntraClaimsMapper : IEntraClaimsMapper
{
    public (ExternalUser ExternalUser, ProfileSeed ProfileSeed) Map(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var externalId = principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Entra token is missing the object identifier claim.");

        var displayName = principal.FindFirstValue("name")
            ?? principal.FindFirstValue(ClaimTypes.Name)
            ?? externalId;

        var email = principal.FindFirstValue("preferred_username")
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue(ClaimTypes.Upn)
            ?? string.Empty;

        return (
            new ExternalUser("AzureAd", externalId),
            new ProfileSeed(displayName, email));
    }
}
