using Blabber.Emulator.Options;

namespace Blabber.Emulator.Configuration;

public sealed class BlabberEmulatorHostedServiceConfig
{
    public BlabberEmulatorOptions Options { get; init; } = new();
}
