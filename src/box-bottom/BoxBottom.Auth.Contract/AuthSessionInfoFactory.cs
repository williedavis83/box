using BoxBottom.Auth.Contract.Models;
using BoxBottom.Users.Contract.Models;

namespace BoxBottom.Auth.Contract;

public static class AuthSessionInfoFactory
{
    public static AuthSessionInfo FromProfile(BaseUserProfile profile) =>
        new(
            profile.UserId,
            profile.DisplayName,
            profile.Email,
            BuildInitials(profile.DisplayName, profile.Email));

    public static string BuildInitials(string displayName, string email)
    {
        var trimmedDisplayName = displayName.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedDisplayName))
        {
            var parts = trimmedDisplayName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length >= 2)
            {
                return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
            }

            if (parts.Length == 1 && parts[0].Length >= 2)
            {
                return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[0][1])}";
            }

            if (parts.Length == 1)
            {
                return char.ToUpperInvariant(parts[0][0]).ToString();
            }
        }

        var trimmedEmail = email.Trim();
        return string.IsNullOrWhiteSpace(trimmedEmail)
            ? "?"
            : char.ToUpperInvariant(trimmedEmail[0]).ToString();
    }
}
