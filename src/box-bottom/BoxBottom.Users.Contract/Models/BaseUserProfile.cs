namespace BoxBottom.Users.Contract.Models;

public abstract record BaseUserProfile(
    Guid UserId,
    string DisplayName,
    string Email,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
