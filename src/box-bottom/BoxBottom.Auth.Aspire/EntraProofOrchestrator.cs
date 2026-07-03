using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BoxBottom.Aspire.Orchestration;
using BoxBottom.Auth.Entra;

namespace BoxBottom.Auth.Aspire;

public static class EntraProofOrchestrator
{
    public static void ConfigureBoeStack(StackDefinition boeStack)
    {
        ArgumentNullException.ThrowIfNull(boeStack);

        var usersApi = boeStack["users-api"];
        var env = usersApi.EnvironmentVariables;

        // ClientId, ClientSecret, and Authority come from Key Vault (auth-entra-* secrets).
        // The Key Vault URI is configured in appsettings.Development.json, not per-run env vars.
        env[EntraProofOrchestrationConfiguration.AuthProviderEnvironmentVariable] = EntraAuthProvider.Name;
        env[KeycloakOrchestrationConfiguration.AuthorityEnvironmentVariable] =
            ReadEnvironmentVariable(
                KeycloakOrchestrationConfiguration.AuthorityEnvironmentVariable,
                EntraProofOrchestrationConfiguration.DefaultEntraAuthority);
        env[KeycloakOrchestrationConfiguration.TenantIdEnvironmentVariable] =
            ReadEnvironmentVariable(
                KeycloakOrchestrationConfiguration.TenantIdEnvironmentVariable,
                EntraProofOrchestrationConfiguration.DefaultEntraTenantId);
    }

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
            KeycloakOrchestrationConfiguration.PublicOriginEnvironmentVariable,
            webEndpoint.Property(EndpointProperty.Url));
    }

    private static string ReadEnvironmentVariable(string name, string fallback) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : fallback;
}
