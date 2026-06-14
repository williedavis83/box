using Blabber.Emulator.Configuration;
using Blabber.Emulator.Options;
using BoxBottom.Emulation.Shared;

namespace Blabber.Emulator;

public static class BleebEmulatorOrchestrationConfiguration
{
    public const string ResourceName = "bleeb-emulator-api";
    public const string ApiProjectPath = @"..\..\box-content\Bleeb.Emulator.Api\Bleeb.Emulator.Api.csproj";
    public const string EmulatorBaseUriEnvironmentVariable = "BlabberEmulator__BleebEmulatorBaseUri";
}
