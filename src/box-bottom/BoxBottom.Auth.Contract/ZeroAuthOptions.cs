namespace BoxBottom.Auth.Contract;

public sealed class ZeroAuthOptions
{
    public const string SectionName = "Auth:ZeroAuth";

    public string[] AllowedProviders { get; set; } = ["ZeroAuth", "test"];
}
