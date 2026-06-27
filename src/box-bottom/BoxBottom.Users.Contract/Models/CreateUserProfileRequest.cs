namespace BoxBottom.Users.Contract.Models;

public sealed record CreateUserProfileRequest(
    string DisplayName,
    string Email);
