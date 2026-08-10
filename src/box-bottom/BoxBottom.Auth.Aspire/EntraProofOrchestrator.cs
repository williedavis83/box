using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BoxBottom.Aspire.Orchestration;

namespace BoxBottom.Auth.Aspire;

public static class EntraProofOrchestrator
{
    public static void WireRealEntraToUsersApi(
        IReadOnlyDictionary<string, StackResources> stacks)
    {
        ArgumentNullException.ThrowIfNull(stacks);

        if (!stacks.TryGetValue(EntraProofOrchestrationConfiguration.BoeStackName, out var stackResources)
            || !stackResources.Apis.TryGetValue("users-api", out var usersApi))
        {
            return;
        }

        var webEndpoint = stackResources.Web.GetEndpoint("http");

        usersApi.WithEnvironment(
            EntraProofOrchestrationConfiguration.PublicOriginEnvironmentVariable,
            webEndpoint.Property(EndpointProperty.Url));
    }
}
