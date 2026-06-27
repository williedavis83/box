using BoxBottom.Auth.Contract.Models;
using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Auth.Entra;

public interface IEntraClaimsMapper
{
    (ExternalUser ExternalUser, ProfileSeed ProfileSeed) Map(System.Security.Claims.ClaimsPrincipal principal);
}
