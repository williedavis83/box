namespace BoxBottom.Auth.Contract;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string Provider { get; set; } = string.Empty;
}
