# Key Vault — secrets and access model

## Current state

- Key Vault **`rdbox-kv`** exists in `rdbox-rg` (created by Terraform, RBAC-authorized).
- `Microsoft.KeyVault` resource provider is **Registered**.
- `users-api` loads Entra secrets from Key Vault when `KeyVault:VaultUri` is set
  ([`KeyVaultConfigurationExtensions`](../../../src/box-top/BoxTop.Users.Api/KeyVaultConfigurationExtensions.cs)).
  The URI comes from `appsettings.Development.json`. `bob` selects ZeroAuth before
  startup and skips the Entra vault provider; `box` applies its transmitted Keycloak
  configuration after Key Vault so emulation values take precedence.

## Recommended vault

| Setting | Value |
|---------|-------|
| Name | `rdbox-kv` (or `kv-rdbox-{unique}` if taken globally) |
| Resource group | `rdbox-rg` |
| Region | `eastus2` (align with RG) |
| Authorization | **RBAC** (`enableRbacAuthorization = true`) — avoid legacy access policies unless required |
| Soft delete / purge protection | Enable for non-dev; optional for throwaway proof |

## Secrets to store

### Phase 1 — Entra proof

| Secret name | Source | Consumed as |
|-------------|--------|-------------|
| `auth-entra-client-secret` | Terraform `azuread_application_password` | `Auth:Entra:ClientSecret` |
| `auth-entra-client-id` | Terraform (`azuread_application.client_id`) | `Auth:Entra:ClientId` |
| `auth-entra-authority` | Terraform variable | `Auth:Entra:Authority` |

The `auth-entra-` prefix and the secret-name → config-key mapping are enforced by
[`BoxKeyVaultSecretManager`](../../../src/box-top/BoxTop.Users.Api/Configuration/BoxKeyVaultSecretManager.cs).

Non-secret config can stay in App Service settings or Aspire parameters:

| Setting | Example | Secret? |
|---------|---------|---------|
| `Auth__Provider` | `Entra` | No |
| `Auth__Entra__TenantId` | `9af8af7b-10ee-4bd5-b71c-20daa8e37878` | No |
| `Auth__Entra__PublicOrigin` | `https://box-proof...` | No |
| `Auth__Entra__CallbackPath` | `/api/signin-oidc` | No |
| `Auth__Entra__ExternalCallbackPath` | `/api/users/signin-oidc` | No |

### Phase 2 — data plane

| Secret name | Purpose |
|-------------|---------|
| `azure-table-users-connection-string` | Alternative to managed identity + URI |
| `azure-table-users-service-uri` | If not using connection string |

Today `BoxTop.Users.Api` uses **ServiceUri** + likely `DefaultAzureCredential` for tables — prefer **managed identity** over connection strings when hosting on Azure.

### Phase 3 — social IdPs (long term)

Store IdP client secrets in Key Vault; map into External ID portal or Graph config (see [social-login-roadmap.md](./social-login-roadmap.md)):

| Secret name | IdP |
|-------------|-----|
| `entra-idp-google-client-secret` | Google |
| `entra-idp-facebook-app-secret` | Facebook |
| `entra-idp-microsoft-consumer-secret` | Microsoft personal accounts |

External ID holds the IdP configuration; Key Vault holds credentials Terraform/scripts push into that config.

## RBAC assignments

| Identity | Role | Scope |
|----------|------|-------|
| Human admin / Terraform SP | Key Vault Secrets Officer | `rdbox-kv` |
| `users-api` managed identity | Key Vault Secrets User | `rdbox-kv` |
| CI deployment SP | Key Vault Secrets Officer (deploy time only) | `rdbox-kv` |

Signed-in user currently has **Owner** on subscription — sufficient for bootstrap; narrow to vault-scoped roles for runtime identities.

## Runtime binding in ASP.NET Core

### Option A — Key Vault references (App Service / Container Apps)

```text
Auth__Entra__ClientSecret = @Microsoft.KeyVault(SecretUri=https://rdbox-kv.vault.azure.net/secrets/auth-entra-client-secret/)
```

### Option B — `Azure.Extensions.AspNetCore.Configuration.Secrets` (implemented)

`BoxTop.Users.Api` uses this. `AddBoxKeyVaultConfiguration` adds the vault as a
configuration source only when `KeyVault:VaultUri` is non-empty, using
`DefaultAzureCredential` and a custom secret manager:

```csharp
var vaultUri = builder.Configuration["KeyVault:VaultUri"];
if (string.IsNullOrWhiteSpace(vaultUri)) { return builder; }
builder.Configuration.AddAzureKeyVault(
    new Uri(vaultUri),
    new DefaultAzureCredential(),
    new BoxKeyVaultSecretManager());
```

`BoxKeyVaultSecretManager` only loads `auth-entra-*` secrets and maps them to config keys:

| Vault secret | Configuration key |
|--------------|-------------------|
| `auth-entra-client-secret` | `Auth:Entra:ClientSecret` |
| `auth-entra-client-id` | `Auth:Entra:ClientId` |
| `auth-entra-authority` | `Auth:Entra:Authority` |

Environment variable overrides still work via `Auth__Entra__*` (double underscore).

### Option C — local Aspire against real Entra (implemented as the `boe` stack)

`boe` runs locally against real Entra with no Keycloak and no committed secrets: it sets
`KeyVault:VaultUri` in `appsettings.Development.json` and relies on `az login` for
`DefaultAzureCredential`. `box` overrides the loaded Entra values through the emulation
configuration document; `bob` selects ZeroAuth and skips the Entra vault provider.

## Rotation

1. Terraform taints `azuread_application_password` → new password → update Key Vault secret version.
2. Restart `users-api` (or wait for App Service slot swap).
3. Old secret invalid after Entra app password deletion policy.

Document rotation in `infra/entra-proof/README.md` when Terraform lands.

## Security checklist

- [ ] No client secret in `appsettings.json` for deployed environments
- [ ] No secret in Terraform state plaintext (use `write_only` / immediate KV upload; mark sensitive outputs)
- [ ] Key Vault network: public access + RBAC for proof; private endpoint for production
- [ ] Audit logging enabled on Key Vault
- [ ] Separate vault per environment (proof vs prod) when moving beyond RD-Box

## Verification commands

```powershell
az keyvault list -g rdbox-rg -o table
az keyvault secret list --vault-name rdbox-kv -o table
# show secret names only, not values:
az keyvault secret list --vault-name rdbox-kv --query "[].name" -o tsv
```
