using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Controllers;
using BoxPack.Users.Customization.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoxPack.Auth.Customization.Controllers;

public sealed class AuthController(IAuthProviderGuard authProviderGuard)
    : AuthControllerBase<UserProfile>
{
    protected override IAuthProviderGuard AuthProviderGuard => authProviderGuard;
}
