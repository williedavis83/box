using System.Text.Json;
using BoxBottom.Auth.Aspire;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class KeycloakOrchestrationConfigurationTests
{
    [Fact]
    public void BuildAuthorityUri_IncludesRealmName()
    {
        var authority = KeycloakOrchestrationConfiguration.BuildAuthorityUri("http://localhost:8080");

        Assert.Equal("http://localhost:8080/realms/box", authority);
    }

    [Fact]
    public void BuildAuthorityUri_UsesExplicitRealm()
    {
        var authority = KeycloakOrchestrationConfiguration.BuildAuthorityUri(
            "http://localhost:8080",
            KeycloakOrchestrationConfiguration.Bok);

        Assert.Equal("http://localhost:8080/realms/bok", authority);
    }

    [Fact]
    public void BuildHealthCheckPath_UsesRealmEndpoint()
    {
        Assert.Equal("/realms/box", KeycloakOrchestrationConfiguration.BuildHealthCheckPath());
        Assert.Equal("/realms/bok", KeycloakOrchestrationConfiguration.BuildHealthCheckPath("bok"));
    }

    [Fact]
    public void ContainerMemoryLimit_IsConfigured()
    {
        Assert.Equal("1g", KeycloakOrchestrationConfiguration.ContainerMemoryLimit);
        Assert.Contains("MaxRAMPercentage=70", KeycloakOrchestrationConfiguration.JavaOptsKcHeap, StringComparison.Ordinal);
    }

    [Fact]
    public void ImportedRealms_IncludesBoxAndBok()
    {
        Assert.Contains(KeycloakOrchestrationConfiguration.ImportedRealms, r => r.RealmName == "box");
        Assert.Contains(KeycloakOrchestrationConfiguration.ImportedRealms, r => r.RealmName == "bok");
        Assert.Equal(2, KeycloakOrchestrationConfiguration.ImportedRealms.Count);
    }

    [Fact]
    public void GetRealm_ResolvesRegisteredRealms()
    {
        Assert.Same(KeycloakOrchestrationConfiguration.Box, KeycloakOrchestrationConfiguration.GetRealm("box"));
        Assert.Same(KeycloakOrchestrationConfiguration.Bok, KeycloakOrchestrationConfiguration.GetRealm("BOK"));
    }

    [Fact]
    public void GetRealm_UnknownRealm_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => KeycloakOrchestrationConfiguration.GetRealm("missing"));
    }

    [Fact]
    public void ImportedRealmJsonFiles_ExistOnDisk()
    {
        var keycloakDir = FindKeycloakImportDirectory();

        foreach (var realm in KeycloakOrchestrationConfiguration.ImportedRealms)
        {
            var path = Path.Combine(keycloakDir, realm.ImportFileName);
            Assert.True(File.Exists(path), $"Missing realm import file: {path}");

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(realm.RealmName, document.RootElement.GetProperty("realm").GetString());
            Assert.Equal(
                realm.ClientId,
                document.RootElement.GetProperty("clients")[0].GetProperty("clientId").GetString());
        }
    }

    private static string FindKeycloakImportDirectory()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var underSrc = Path.Combine(dir.FullName, "src", "box-bottom", "BoxBottom.Auth.Aspire", "keycloak");
            if (Directory.Exists(underSrc))
            {
                return underSrc;
            }

            var sibling = Path.Combine(dir.FullName, "box-bottom", "BoxBottom.Auth.Aspire", "keycloak");
            if (Directory.Exists(sibling))
            {
                return sibling;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not locate BoxBottom.Auth.Aspire/keycloak from the test base directory.");
    }

    [Fact]
    public void BuildAuthEmulationJson_SerializesEntraDocument()
    {
        var json = KeycloakOrchestrator.BuildAuthEmulationJson(
            "http://localhost:8080",
            "http://localhost:49240");

        using var document = JsonDocument.Parse(json);
        var entra = document.RootElement.GetProperty("singletons").GetProperty("Entra");

        Assert.Equal("Entra", entra.GetProperty("provider").GetString());
        Assert.Equal("box", entra.GetProperty("tenantId").GetString());
        Assert.Equal("http://localhost:8080/realms/box", entra.GetProperty("authority").GetString());
        Assert.Equal("box-web", entra.GetProperty("clientId").GetString());
        Assert.Equal("box-web-secret", entra.GetProperty("clientSecret").GetString());
        Assert.Equal("http://localhost:49240", entra.GetProperty("publicOrigin").GetString());
        Assert.False(document.RootElement.TryGetProperty("lists", out _));
        Assert.False(document.RootElement.TryGetProperty("dictionaries", out _));
    }

    [Fact]
    public void BuildAuthEmulationJson_UsesSelectedRealmAndStackPublicOrigin()
    {
        var json = KeycloakOrchestrator.BuildAuthEmulationJson(
            "http://localhost:8080",
            "http://localhost:51001",
            KeycloakOrchestrationConfiguration.Bok);

        using var document = JsonDocument.Parse(json);
        var entra = document.RootElement.GetProperty("singletons").GetProperty("Entra");

        Assert.Equal("bok", entra.GetProperty("tenantId").GetString());
        Assert.Equal("http://localhost:8080/realms/bok", entra.GetProperty("authority").GetString());
        Assert.Equal("bok-web", entra.GetProperty("clientId").GetString());
        Assert.Equal("bok-web-secret", entra.GetProperty("clientSecret").GetString());
        Assert.Equal("http://localhost:51001", entra.GetProperty("publicOrigin").GetString());
    }

    [Fact]
    public void StackBindings_CanSelectDistinctRealmsPerStack()
    {
        var boxBinding = new KeycloakStackBinding("box", KeycloakOrchestrationConfiguration.Box);
        var bokBinding = new KeycloakStackBinding("bok", KeycloakOrchestrationConfiguration.Bok);

        Assert.Equal("box", boxBinding.Realm.RealmName);
        Assert.Equal("bok", bokBinding.Realm.RealmName);
        Assert.NotEqual(boxBinding.Realm.ClientId, bokBinding.Realm.ClientId);
        Assert.NotEqual(boxBinding.Realm.ClientSecret, bokBinding.Realm.ClientSecret);
    }
}
