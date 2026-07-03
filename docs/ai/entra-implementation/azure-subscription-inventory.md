# Azure subscription inventory (RD-Box)

Captured with Azure CLI from the active session on **2026-07-01**. Re-run the commands below after infrastructure changes.

## Subscription and tenant

| Property | Value |
|----------|-------|
| Subscription name | **RD-Box** |
| Subscription ID | `acf84f87-9597-43fd-9994-761c094389fa` |
| Tenant display name | **rdbox** |
| Tenant ID | `9af8af7b-10ee-4bd5-b71c-20daa8e37878` |
| Default domain | `rdbox.onmicrosoft.com` |
| **Tenant type** | **`CIAM`** (Microsoft Entra External ID) |
| Signed-in user | `willie.davis@sendto.global` (guest: `willie.davis_sendto.global#EXT#@rdbox.onmicrosoft.com`) |

```powershell
az account show
az rest --method GET --uri "https://graph.microsoft.com/v1.0/organization" `
  --query "value[0].{displayName:displayName, tenantType:tenantType, id:id, verifiedDomains:verifiedDomains[].name}"
```

### Implication

This is **not** a workforce Entra ID tenant. Social/customer identity features live in **Entra External ID**. Application authority URLs should use:

```text
https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0
```

(not `https://login.microsoftonline.com/{tenantId}/v2.0` unless Microsoft documents otherwise for a specific flow).

## Resource groups and resources

| Resource group | Location | Resources |
|----------------|----------|-----------|
| `rdbox-rg` | `eastus2` (RG metadata) | `rgboxtest` — `Microsoft.Storage/storageAccounts` in **eastus** |

```powershell
az group list -o table
az resource list -g rdbox-rg -o table
```

### Storage (already used by box)

| Name | Table endpoint | Notes |
|------|----------------|-------|
| `rgboxtest` | `https://rgboxtest.table.core.windows.net/` | Matches `BoxTop.Users.Api` `AzureTableAccounts:table-users:ServiceUri` |

```powershell
az storage account list -g rdbox-rg -o json
```

## Key Vault

**None deployed** in this subscription.

```powershell
az keyvault list -o table
```

- `Microsoft.KeyVault` provider: **Registered**
- Probed name `rdbox-kv`: **not found**

## Entra / External ID applications

Only the system application created for External ID user storage:

| Display name | App ID | signInAudience |
|--------------|--------|----------------|
| b2c-extensions-app. Do not modify. Used by AADB2C for storing user data. | `f77d2747-a729-4db7-a130-be87b3206121` | AzureADMyOrg |

```powershell
az ad app list -o table
az rest --method GET --uri "https://graph.microsoft.com/v1.0/applications" -o json
```

**No `box-web` (or similar) app registration exists yet.**

Graph probes for identity providers and user flow attributes returned **AADB2C / permission** errors with the current CLI login — configure flows in the **Entra External ID** portal or grant Graph permissions to the automation identity used by Terraform.

## RBAC (signed-in user)

| Role | Scope |
|------|-------|
| Owner | `/subscriptions/acf84f87-9597-43fd-9994-761c094389fa` |
| Storage Table Data Contributor | `.../storageAccounts/rgboxtest` |

```powershell
$oid = az ad signed-in-user show --query id -o tsv
az role assignment list --assignee $oid --all -o table
```

Sufficient to create Key Vault, Terraform state storage, and Entra app registrations via the `azuread` provider.

## Provider registration

| Namespace | State |
|-----------|-------|
| Microsoft.KeyVault | Registered |
| Microsoft.Web | Registered |
| Microsoft.App | **Not registered** |

Register `Microsoft.App` before Container Apps if hosting on ACA:

```powershell
az provider register --namespace Microsoft.App
```

## Other subscriptions visible (not default)

| Name | Tenant ID |
|------|-----------|
| root sub | `f5e49cc3-d0c4-4f56-ab1f-1baf19f36783` |
| VirtualPersonalMailbox | `1431e04e-cbd8-491d-a6a5-9f37d3715d52` |
| sendto.global dev sub | `c17b871c-34e8-4c29-a823-dfa7d7d78306` |

Entra proof work should stay in **RD-Box** unless product requirements say otherwise.

## Gaps vs. box-chassis needs

| Need | Status |
|------|--------|
| Entra External ID app registration for box web | **Missing** |
| Client secret / cert for confidential client | **Missing** |
| Key Vault for secrets | **Missing** |
| Terraform definitions | **Missing** (no `.tf` in repo) |
| Hosted public HTTPS origin for OIDC redirect | **Missing** (local Aspire + Keycloak only today) |
| User flow (sign-up/sign-in) | **Not verified** (portal setup required) |
| Social IdPs (Google, etc.) | **Not configured** |
| Playwright E2E for box Entra login | **Missing** (bob ZeroAuth only) |
