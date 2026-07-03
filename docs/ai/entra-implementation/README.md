# Entra External ID — reference implementation

This folder documents how box-chassis authenticates against **real Microsoft Entra
External ID** (the `boe` stack), using the **RD-Box** Azure subscription. It doubles as a
worked example of taking an emulated capability (Keycloak) to a real cloud dependency
managed by **Terraform** with secrets in **Key Vault**.

The near-term target is email/password sign-in in a CIAM tenant; the long-term target is
customer-facing sign-in with social identity providers (see
[social-login-roadmap.md](./social-login-roadmap.md)).

## What is implemented

| Component | Location |
|-----------|----------|
| Terraform (Key Vault + app registration) | [`infra/entra-proof/`](../../../infra/entra-proof/) |
| Portal runbook | [`infra/entra-proof/README.md`](../../../infra/entra-proof/README.md) |
| `boe` Aspire stack | [`Orchestrator.cs`](../../../src/box-pack/BoxPack.Aspire.Orchestration/Orchestrator.cs), [`EntraProofOrchestrator.cs`](../../../src/box-bottom/BoxBottom.Auth.Aspire/EntraProofOrchestrator.cs) |
| Key Vault config provider | [`KeyVaultConfigurationExtensions.cs`](../../../src/box-top/BoxTop.Users.Api/KeyVaultConfigurationExtensions.cs), [`BoxKeyVaultSecretManager.cs`](../../../src/box-top/BoxTop.Users.Api/Configuration/BoxKeyVaultSecretManager.cs) |
| OIDC + cookie auth | [`EntraAuthServiceCollectionExtensions.cs`](../../../src/box-bottom/BoxBottom.Auth.Entra/EntraAuthServiceCollectionExtensions.cs) |
| Claims → user | [`EntraClaimsMapper.cs`](../../../src/box-bottom/BoxBottom.Auth.Entra/EntraClaimsMapper.cs) |
| Playwright proof | [`auth-entra-ui.spec.js`](../../../src/box-test/BoxTest.Playwright/tests/auth-entra-ui.spec.js) |
| CI | [`.github/workflows/ci.yml`](../../../.github/workflows/ci.yml) |

## Running the `boe` stack

No per-run environment variables are needed. The Key Vault URI lives in
`BoxTop.Users.Api/appsettings.Development.json`, and the client id, client secret, and
authority are read from Key Vault secrets at runtime. You only need to be signed in to
Azure so `DefaultAzureCredential` can read the vault:

```powershell
az login
dotnet run --project src/box-top/BoxTop.Aspire
```

The `box` (Keycloak) and `bob` (ZeroAuth) stacks disable Key Vault and require no Azure
credentials. See [box-application-wiring.md](./box-application-wiring.md) for the full
configuration flow, and [`infra/entra-proof/README.md`](../../../infra/entra-proof/README.md)
for `terraform apply` and the External ID portal steps.

## Stack layout

| Stack | Auth | Notes |
|-------|------|-------|
| `box` | Keycloak emulation | Default local dev; no Azure needed |
| `bob` | ZeroAuth | Dev JSON login; no Azure needed |
| `boe` | Real Entra External ID | Reads Key Vault; requires `az login` |

## Documents

| Doc | Purpose |
|-----|---------|
| [azure-subscription-inventory.md](./azure-subscription-inventory.md) | What exists in the RD-Box subscription (`az` interrogation, 2026-07-01) |
| [proof-criteria.md](./proof-criteria.md) | Definition of done for "Entra actually works" |
| [terraform.md](./terraform.md) | Infrastructure provisioned with Terraform |
| [keyvault.md](./keyvault.md) | Secret naming, access model, and runtime binding |
| [entra-external-id-setup.md](./entra-external-id-setup.md) | App registration, authority URLs, redirect URIs, user flows |
| [box-application-wiring.md](./box-application-wiring.md) | How Azure resources map to `Auth:Entra` config in box |
| [social-login-roadmap.md](./social-login-roadmap.md) | Path from email login to full social IdPs |
