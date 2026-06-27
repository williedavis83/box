using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Users.Contract;

public interface IExternalUserMapper<TExternalUser>
{
    ExternalUser Map(TExternalUser externalUser);
}
