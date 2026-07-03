# Key Vault — secrets and access model

## Current state

- **No Key Vault** in RD-Box subscription (verified 2026-07-01).
- `Microsoft.KeyVault` resource provider is **Registered**.
- `users-api` today reads Entra settings from **environment variables** / `appsettings.json` (Keycloak overrides in local Aspire).

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
| `auth-entra-client-secret` | Terraform `azuread_application_password` | `Auth__Entra__ClientSecret` |
| `auth-entra-client-id` | Terraform output (optional; can be non-secret env) | `Auth__Entra__ClientId` |
| `auth-entra-authority` | Terraform variable / output | `Auth__Entra__Authority` |

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

### Option B — `Azure.Extensions.AspNetCore.Configuration.Secrets`

In `BoxTop.Users.Api/Program.cs` (future change):

```csharp
if (!builder.Environment.IsDevelopment())
{
    var vaultUri = builder.Configuration["KeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(vaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(vaultUri),
            new DefaultAzureCredential());
    }
}
```

Map vault secret names to configuration keys:

| Vault secret | Configuration key |
|--------------|-------------------|
| `auth-entra-client-secret` | `Auth:Entra:ClientSecret` |

Environment variable override remains `Auth__Entra__ClientSecret` (double underscore).

### Option C — Aspire (local against real Entra)

For developer testing against real Entra without Keycloak:

- Add optional `WithAzureKeyVault` reference in orchestration, **or**
- User secrets / `.env` locally (never commit)
- Keep Keycloak as default local path

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
