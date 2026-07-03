# Entra proof infrastructure (Terraform)

Provisions Key Vault and the `box-web` Entra External ID app registration for the **boe** Aspire stack proof.

## Prerequisites

- [Terraform](https://www.terraform.io/downloads) >= 1.5
- Azure CLI logged in to **RD-Box** subscription
- Permissions: Owner on subscription (or Contributor + ability to create app registrations)

```powershell
az login
az account set --subscription "RD-Box"
```

## Apply

```powershell
cd infra/entra-proof
copy terraform.tfvars.example terraform.tfvars   # optional overrides
terraform init
terraform plan
terraform apply
```

## Running the boe stack

No per-run environment variables are required. The vault URI is configured in
`src/box-top/BoxTop.Users.Api/appsettings.Development.json` (`KeyVault:VaultUri`),
and the client id, client secret, and authority are read from Key Vault secrets
(`auth-entra-client-id`, `auth-entra-client-secret`, `auth-entra-authority`) at
runtime. You only need to be signed in to Azure:

```powershell
az login
dotnet run --project src/box-top/BoxTop.Aspire
```

The **box** (Keycloak) and **bob** (ZeroAuth) stacks disable Key Vault and run
without any Azure credentials; only the **boe** stack reads from Key Vault.

If the vault name differs from `rdbox-kv`, update `KeyVault:VaultUri` in
`appsettings.Development.json` (or override `KeyVault__VaultUri` for a single run):

```powershell
terraform output key_vault_uri
```

---

## External ID portal checklist (manual)

Complete after `terraform apply` creates the app registration.

### 1. User flow

1. Open [Microsoft Entra admin center](https://entra.microsoft.com) → **External Identities** (or **Entra External ID**).
2. **User flows** → **New user flow**.
3. Type: **Sign up and sign in**.
4. Name: e.g. `SignUpSignIn`.
5. Identity providers: **Email with password** (local account).
6. User attributes: **Display Name**, **Email Address**.
7. Application claims (token configuration): enable **Object ID**, **Display Name**, **Email Addresses**.

### 2. Link app to user flow

1. Open the user flow → **Applications** → **Add application**.
2. Select **box-web** (created by Terraform). It appears under **Enterprise
   applications** because Terraform also creates the service principal
   (`azuread_service_principal`); a bare app registration would not be listed here.
3. Save.

### 3. Verify OIDC metadata

```powershell
Invoke-RestMethod "https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0/.well-known/openid-configuration" |
  Select-Object issuer, authorization_endpoint
```

### 4. Proof test user

Create a dedicated test user in External ID (e.g. via sign-up flow or portal). Store credentials in local user secrets or CI variables — **never commit**:

- `ENTRA_PROOF_EMAIL`
- `ENTRA_PROOF_PASSWORD`

### 5. Redirect URI note

Terraform registers `http://localhost/api/users/signin-oidc` (portless loopback). At runtime the app sends `{PublicOrigin}/api/users/signin-oidc` which may include a port; Entra matches via loopback wildcard rules.

---

## Key Vault secrets

| Secret name | Maps to config |
|-------------|----------------|
| `auth-entra-client-secret` | `Auth:Entra:ClientSecret` |
| `auth-entra-client-id` | (optional; can use terraform output) |
| `auth-entra-authority` | (optional; can use appsettings default) |

## Troubleshooting

### `InvalidAccessTokenVersion` on app creation

```
Error: Could not create application
unexpected status 400 ... InvalidAccessTokenVersion: Access Token Accepted
Version may not be 1 or null. paramName: AccessTokenAcceptedVersion
```

Entra External ID (CIAM) rejects v1 access tokens. The `azuread_application`
must request v2 tokens:

```hcl
api {
  requested_access_token_version = 2
}
```

This is already set in `entra-app.tf`. Re-run `terraform apply` after adding it —
any resources created before the failure (Key Vault, role assignment, authority
secret) are left in place and the run resumes idempotently.

## Destroy

```powershell
terraform destroy
```

Note: Key Vault soft-delete may retain the vault name for 7 days.
