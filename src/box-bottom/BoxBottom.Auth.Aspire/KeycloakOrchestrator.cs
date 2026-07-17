using System.Text.Json;
using System.Text.Json.Serialization;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BoxBottom.Aspire.Orchestration;
using BoxBottom.Auth.Emulation;
using BoxBottom.Auth.Emulation.Configuration;

namespace BoxBottom.Auth.Aspire;

public static class KeycloakOrchestrator
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

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
                .WithEnvironment("JAVA_OPTS_KC_HEAP", KeycloakOrchestrationConfiguration.JavaOptsKcHeap)
                .WithContainerRuntimeArgs("--memory", KeycloakOrchestrationConfiguration.ContainerMemoryLimit)
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
            var keycloakUrl = keycloakEndpoint.Property(EndpointProperty.Url);
            var webUrl = webEndpoint.Property(EndpointProperty.Url);

            usersApi
                .WithReference(keycloakEndpoint)
                .WaitFor(_keycloakBuilder)
                .WithEnvironment(async context =>
                {
                    // Endpoints are Aspire references until this callback. Resolve (or take the
                    // manifest expression), put real strings on the document, then serialize.
                    string keycloakHttpUrl;
                    string publicOrigin;

                    if (context.ExecutionContext.IsPublishMode)
                    {
                        keycloakHttpUrl = ((IManifestExpressionProvider)keycloakUrl).ValueExpression;
                        publicOrigin = ((IManifestExpressionProvider)webUrl).ValueExpression;
                    }
                    else
                    {
                        keycloakHttpUrl = await keycloakUrl.GetValueAsync(context.CancellationToken)
                            .ConfigureAwait(false)
                            ?? throw new InvalidOperationException("Keycloak HTTP URL could not be resolved.");
                        publicOrigin = await webUrl.GetValueAsync(context.CancellationToken)
                            .ConfigureAwait(false)
                            ?? throw new InvalidOperationException("Web HTTP URL could not be resolved.");
                    }

                    context.EnvironmentVariables[EntraEmulationRegistryExtensions.EntraEmulationKey] =
                        BuildAuthEmulationJson(keycloakHttpUrl, publicOrigin);
                });
        }
    }

    public static string BuildAuthEmulationJson(string keycloakHttpUrl, string publicOrigin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keycloakHttpUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicOrigin);

        var document = new
        {
            singletons = new Dictionary<string, EntraEmulatorAnchorConfig>(StringComparer.OrdinalIgnoreCase)
            {
                [EntraEmulationRegistryExtensions.EntraAnchorName] = new EntraEmulatorAnchorConfig
                {
                    Provider = "Entra",
                    TenantId = KeycloakOrchestrationConfiguration.RealmName,
                    Authority = KeycloakOrchestrationConfiguration.BuildAuthorityUri(keycloakHttpUrl),
                    ClientId = KeycloakOrchestrationConfiguration.ClientId,
                    ClientSecret = KeycloakOrchestrationConfiguration.ClientSecret,
                    PublicOrigin = publicOrigin,
                },
            },
        };

        return JsonSerializer.Serialize(document, SerializerOptions);
    }

    internal static void ResetForTests() => _keycloakBuilder = null;
}
