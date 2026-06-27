using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Contract.Models;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Models;

namespace BoxPack.Auth.Customization.Mapping;

public sealed class ExternalUserIdentityMapper : IExternalUserMapper<ExternalUser>
{
    public ExternalUser Map(ExternalUser externalUser) => externalUser;
}
