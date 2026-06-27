using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BoxBottom.Aspire.Orchestration;

namespace BoxBottom.Auth.Aspire;

public static class KeycloakOrchestrator
{
    private static IResourceBuilder<ContainerResource>? _keycloakBuilder;

    public static IResourceBuilder<ContainerResource> Orchestrate(StackOperations stackOperations)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);

        if (_keycloakBuilder is not null)
        {
            return _keycloakBuilder;
        }

        var realmImportPath = Path.Combine(AppContext.BaseDirectory, "keycloak");

        _keycloakBuilder = stackOperations.OrchestrateSupport(builder =>
        {
            var container = builder
                .AddContainer(KeycloakOrchestrationConfiguration.ResourceName, "quay.io/keycloak/keycloak", "26.0.5")
                .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
                .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
                .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
                .WithEnvironment("KC_HEALTH_ENABLED", "true")
                .WithEnvironment("KC_METRICS_ENABLED", "true")
                .WithBindMount(realmImportPath, "/opt/keycloak/data/import")
                .WithArgs("start-dev", "--import-realm");

            return container.WithHttpHealthCheck(
                KeycloakOrchestrationConfiguration.BuildHealthCheckPath(),
                200,
                "http");
        });

        return _keycloakBuilder;
    }

    public static void WireEntraEmulationToUsersApi(
        StackOperations stackOperations,
        IReadOnlyDictionary<string, StackResources> stacks,
        params string[] stackNames)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);
        ArgumentNullException.ThrowIfNull(stacks);
        ArgumentNullException.ThrowIfNull(stackNames);

        if (_keycloakBuilder is null)
        {
            return;
        }

        foreach (var stackName in stackNames)
        {
            if (!stacks.TryGetValue(stackName, out var stackResources)
                || !stackResources.Apis.TryGetValue("users-api", out var usersApi))
            {
                continue;
            }

            var keycloakEndpoint = _keycloakBuilder.GetEndpoint("http");
            var webEndpoint = stackResources.Web.GetEndpoint("http");

            usersApi
                .WithReference(keycloakEndpoint)
                .WaitFor(_keycloakBuilder)
                .WithEnvironment(
                    KeycloakOrchestrationConfiguration.AuthorityEnvironmentVariable,
                    ReferenceExpression.Create(
                        $"{keycloakEndpoint.Property(EndpointProperty.Url)}/realms/{KeycloakOrchestrationConfiguration.RealmName}"))
                .WithEnvironment(
                    KeycloakOrchestrationConfiguration.PublicOriginEnvironmentVariable,
                    webEndpoint.Property(EndpointProperty.Url))
                .WithEnvironment(
                    KeycloakOrchestrationConfiguration.TenantIdEnvironmentVariable,
                    KeycloakOrchestrationConfiguration.RealmName)
                .WithEnvironment(
                    KeycloakOrchestrationConfiguration.ClientIdEnvironmentVariable,
                    KeycloakOrchestrationConfiguration.ClientId)
                .WithEnvironment(
                    KeycloakOrchestrationConfiguration.ClientSecretEnvironmentVariable,
                    KeycloakOrchestrationConfiguration.ClientSecret);
        }
    }

    internal static void ResetForTests() => _keycloakBuilder = null;
}
