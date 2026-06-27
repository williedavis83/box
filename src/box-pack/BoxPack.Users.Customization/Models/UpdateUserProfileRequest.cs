using BoxBottom.Users.Contract.Models;

namespace BoxPack.Users.Customization.Models;

public sealed record UpdateUserProfileRequest(
    string DisplayName,
    string Email)
    : BaseUpdateUserProfileRequest;
