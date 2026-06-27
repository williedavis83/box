using BoxBottom.Azure.Table;
using BoxBottom.Auth.Contract;
using BoxBottom.Emulation;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Controllers;
using BoxPack.Users.Customization.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

[assembly: EmulationAnchor(UserTableKeys.Users, typeof(IAzureTableService), EmulationAnchorKind.Singleton)]

namespace BoxPack.Users.Customization.Controllers;

public sealed class UserProfileController(
    IUserProfileService<UserProfile, UpdateUserProfileRequest> userProfileService,
    [FromKeyedServices(UserTableKeys.Users)] IAzureTableService usersTableService,
    ICurrentUserAccessor currentUserAccessor)
    : UserProfileControllerBase<UserProfile, UpdateUserProfileRequest>(
        userProfileService,
        usersTableService,
        currentUserAccessor);
