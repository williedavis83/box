# Box application wiring

How box-chassis code consumes Entra settings and what must change for real Azure proof.

## Configuration model

`EntraAuthOptions` (`Auth:Entra` section):

| Key | Purpose |
|-----|---------|
| `TenantId` | Entra tenant GUID |
| `Authority` | OIDC authority base URL (**required for CIAM** — do not rely on default) |
| `ClientId` | App registration application (client) ID |
| `ClientSecret` | Confidential client secret (Key Vault in deployed envs) |
| `CallbackPath` | Path on `users-api` after edge rewrite: `/api/signin-oidc` |
| `ExternalCallbackPath` | Browser-facing path via edge: `/api/users/signin-oidc` |
| `PublicOrigin` | Web app origin (scheme + host + port) for redirect URI + post-login redirect |
| `SignedOutCallbackPath` | `/signout-callback-oidc` |

Environment variable equivalents use `__` nesting, e.g. `Auth__Entra__Authority`.

## Authority resolution (important)

```46:48:src/box-bottom/BoxBottom.Auth.Entra/EntraAuthServiceCollectionExtensions.cs
                options.Authority = string.IsNullOrWhiteSpace(entraOptions.Authority)
                    ? $"https://login.microsoftonline.com/{entraOptions.TenantId}/v2.0"
                    : entraOptions.Authority.TrimEnd('/');
```

For RD-Box CIAM tenant, **always set `Authority` explicitly**:

```text
https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0
```

Future code improvement (optional): detect CIAM tenants or document-only for now.

## Redirect URI construction

```68:76:src/box-bottom/BoxBottom.Auth.Entra/EntraAuthServiceCollectionExtensions.cs
                    OnRedirectToIdentityProvider = context =>
                    {
                        var origin = !string.IsNullOrWhiteSpace(entraOptions.PublicOrigin)
                            ? entraOptions.PublicOrigin.TrimEnd('/')
                            : $"{context.Request.Scheme}://{context.Request.Host}";

                        context.ProtocolMessage.RedirectUri =
                            $"{origin}{entraOptions.ExternalCallbackPath}";
```

**Rule:** `PublicOrigin` must be the **web** URL the browser uses, not the edge internal URL. Same requirement as Keycloak local fix.

## Request path flow

```text
Browser → {PublicOrigin}/api/users/Auth/login
       → Edge YARP /api/users/* → users-api
       → OIDC Challenge → Entra authorize
       → Browser → {PublicOrigin}/api/users/signin-oidc
       → Edge → users-api /api/signin-oidc (CallbackPath)
       → Cookie + redirect to {PublicOrigin}/
```

Edge routing: `users-api` mounted at `/api/users/` (see `EdgeRoutingConfiguration`).

## Claims → user profile

```13:28:src/box-bottom/BoxBottom.Auth.Entra/EntraClaimsMapper.cs
        var externalId = principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Entra token is missing the object identifier claim.");
        // ...
        return (
            new ExternalUser("AzureAd", externalId),
            new ProfileSeed(displayName, email));
```

External ID users from social IdPs still expose a stable **`oid`** in the tenant directory — provider string may stay `AzureAd` or evolve to `EntraExternalId` later for clarity.

## Environment matrix

| Environment | Provider | Authority source | Secret source |
|-------------|----------|------------------|---------------|
| Local Aspire (default box) | Entra (emulated) | Keycloak container URL | Keycloak realm secret |
| Local Aspire (**boe**, `BOX_ENTRA_PROOF=1`) | Entra | `ciamlogin.com` | Key Vault (`DefaultAzureCredential`) |
| bob stack | ZeroAuth | N/A | N/A |

## Orchestrator — **boe** stack (implemented)

[`Orchestrator.cs`](../../../src/box-pack/BoxPack.Aspire.Orchestration/Orchestrator.cs) adds a third stack when `BOX_ENTRA_PROOF=1`:

- **box** — Keycloak emulation (unchanged)
- **bob** — ZeroAuth (unchanged)
- **boe** — real Entra External ID via [`EntraProofOrchestrator`](../../../src/box-bottom/BoxBottom.Auth.Aspire/EntraProofOrchestrator.cs)

```powershell
$env:BOX_ENTRA_PROOF = "1"
$env:Auth__Entra__ClientId = "<terraform output entra_client_id>"
$env:KeyVault__VaultUri = "<terraform output key_vault_uri>"
dotnet run --project src/box-top/BoxTop.Aspire
```

`boe` `users-api` receives:

```text
Auth__Provider=Entra
Auth__Entra__Authority=https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0
Auth__Entra__TenantId=9af8af7b-10ee-4bd5-b71c-20daa8e37878
Auth__Entra__ClientId=<from env>
KeyVault__VaultUri=<from env>
Auth__Entra__PublicOrigin=<boe web endpoint, wired post-orchestrate>
Auth__Entra__CallbackPath=/api/signin-oidc
Auth__Entra__ExternalCallbackPath=/api/users/signin-oidc
```

Client secret loads from Key Vault secret `auth-entra-client-secret` via [`BoxKeyVaultSecretManager`](../../../src/box-top/BoxTop.Users.Api/Configuration/BoxKeyVaultSecretManager.cs).

## Frontend

`BoxPack.Users.Web` → `startEntraLogin()` navigates to `/api/users/Auth/login` (proxied through Vite to edge). No frontend change required for real Entra if origins are correct.

User Profile tab: route-scoped registration in `App.vue` (unchanged).

## Storage

Proof environment can continue using existing table storage:

```text
https://rgboxtest.table.core.windows.net/
```

Ensure hosted `users-api` identity has **Storage Table Data Contributor** (or RBAC equivalent) on `rgboxtest`.

## Tests

| Test | File | Status |
|------|------|--------|
| **boe** Entra redirect UI | `auth-entra-ui.spec.js` | Implemented |
| **boe** ZeroAuth rejection | `auth-api.spec.js` | Implemented |
| **boe** orchestration env | `AuthOrchestrationTests.cs` | Implemented |

```javascript
const boeWeb = stackHttp('boe', 'web')
test.skip(!boeWeb, 'boe web is not configured')
```

## Implementation checklist

- [x] `Program.cs`: optional Key Vault configuration provider
- [x] Dedicated **boe** stack (no Keycloak wiring)
- [x] CIAM defaults in `appsettings.json`; secret via Key Vault
- [x] CIAM authority warning in `EntraAuthServiceCollectionExtensions`
- [x] Playwright **boe** Entra E2E
- [x] Terraform + KV in [`infra/entra-proof/`](../../../infra/entra-proof/)
- [ ] Manual: `terraform apply` + External ID portal user flow
