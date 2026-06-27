namespace BoxBottom.Auth.Contract.Models;

public sealed record AuthSessionInfo(
    Guid UserId,
    string DisplayName,
    string Email,
    string Initials);
