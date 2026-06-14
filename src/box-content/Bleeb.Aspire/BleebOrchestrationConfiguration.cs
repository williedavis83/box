using BoxBottom.Aspire.Orchestration;

namespace Bleeb.Aspire;

public static class BleebOrchestrationConfiguration
{
    public const string ResourceName = "bleeb-api";
    public const string PrimaryApiLogicalName = "primary-api";
    public const string BaseUriEnvironmentVariable = "Bleeb__BaseUri";
    public const string ApiProjectPath = @"..\..\box-content\Bleeb.Api\Bleeb.Api.csproj";

    public static string BuildBleebServiceAddress(string aspireResourceName = ResourceName) =>
        EdgeRoutingConfiguration.BuildApiClusterAddress(aspireResourceName);
}
