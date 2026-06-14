namespace Bleeb.Api.Options;

public sealed class BleebAccountsOptions
{
    public const string SectionName = "Bleeb";

    public Dictionary<string, BleebAccountEndpoints> Accounts { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed class BleebAccountEndpoints
{
    public string Bar { get; init; } = string.Empty;

    public string Baz { get; init; } = string.Empty;
}
