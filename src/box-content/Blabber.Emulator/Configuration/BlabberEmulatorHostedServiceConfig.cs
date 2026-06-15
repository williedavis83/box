using Blabber.Emulator.Options;

namespace Blabber.Emulator.Configuration;

public sealed class BlabberEmulatorHostedServiceConfig
{
    public List<BlabberEmulatorAccount> Accounts { get; init; } = [];
}
