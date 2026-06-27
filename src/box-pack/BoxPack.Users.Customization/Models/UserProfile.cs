using BoxBottom.Users.Contract.Models;

namespace BoxPack.Users.Customization.Models;

public record UserProfile(
    Guid UserId,
    string DisplayName,
    string Email,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
    : BaseUserProfile(UserId, DisplayName, Email, CreatedAtUtc, UpdatedAtUtc);
