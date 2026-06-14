namespace Blabber.Emulator.Options;

public sealed class BlabberEmulatorOptions
{
    public const string SectionName = "BlabberEmulator";

    public Uri? BleebEmulatorBaseUri { get; init; }

    public List<BlabberEmulatorAccount> Accounts { get; init; } = [];
}

public sealed class BlabberEmulatorAccount
{
    public string Account { get; init; } = string.Empty;

    public string Bar { get; init; } = string.Empty;

    public string Baz { get; init; } = string.Empty;
}
