# Entra External ID setup (CIAM tenant)

The RD-Box tenant type is **CIAM** (Entra External ID). Setup differs from workforce Entra ID.

## Tenant reference

| Item | Value |
|------|-------|
| Tenant name | rdbox |
| Tenant ID | `9af8af7b-10ee-4bd5-b71c-20daa8e37878` |
| Domain | `rdbox.onmicrosoft.com` |
| OIDC authority (recommended) | `https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0` |
| OIDC metadata | `{authority}/.well-known/openid-configuration` |

Verify metadata after app registration:

```powershell
Invoke-RestMethod "https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0/.well-known/openid-configuration" |
  Select-Object issuer, authorization_endpoint, token_endpoint
```

## Application registration (`box-web`)

Create via Terraform (`azuread` provider) or portal: **Entra External ID → App registrations → New registration**.

| Setting | Value |
|---------|-------|
| Name | `box-web` |
| Supported account types | **Accounts in this organizational directory only** (External ID customers) |
| Redirect URI (Web) | `{PublicOrigin}/api/users/signin-oidc` |

### Redirect URIs to register

Match box edge routing (browser hits edge/web proxy path, not internal API path):

| Environment | PublicOrigin | Redirect URI |
|-------------|--------------|--------------|
| Local Aspire (`boe`) | `http://localhost:{ephemeral}` | `http://localhost/api/users/signin-oidc` |
| Proof/staging host | `https://{host}` | `https://{host}/api/users/signin-oidc` |

Entra allows **any port** on an `http://localhost` loopback redirect, so the single
port-less entry that Terraform registers (`variables.tf`) covers every local Aspire run.

Internal API callback path (after YARP strip) remains `/api/signin-oidc` — configured in `Auth:Entra:CallbackPath`, not sent to Entra as redirect URI.

### Authentication settings

| Setting | Value |
|---------|-------|
| Platform | Web |
| Client secret | Generate; store in Key Vault |
| ID tokens | Enabled (implicit hybrid not required for auth code) |
| Allow public client flows | No |

### API permissions (phase 1)

For basic OIDC sign-in, default **openid/profile/email** scopes are sufficient via user flow. Optional Graph permissions only if app needs to call Graph later.

Do **not** confuse with Microsoft Graph delegated permissions needed for **admin** automation (IdentityUserFlow.ReadWrite.All) — that is for Terraform/ops SP, not `box-web`.

## User flow (sign-up and sign-in)

Configure in **Entra External ID → User flows** (or “Sign-in experience” depending on portal version).

Phase 1 flow:

1. Create flow: **Sign up and sign in**
2. Identity providers: **Email with password** (local account)
3. User attributes: **Display Name**, **Email Address**
4. Application claims (token configuration):
   - `Display Name` → `name`
   - **Email Addresses** → `email` / `preferred_username`
   - **Object ID** → required for `EntraClaimsMapper` (`oid` claim)

5. **Assign** the `box-web` application to this user flow.

### Claims mapping vs. box code

`EntraClaimsMapper` expects:

| Claim | Used for |
|-------|----------|
| `oid` (object identifier) | `ExternalUser.ExternalId` with provider `AzureAd` |
| `name` | Profile display name |
| `preferred_username` / email | Profile email |

Ensure the user flow emits **Object ID** and email claims in the ID token.

## Link app to user flow

Without this assignment, authorize requests may fail or omit policy context.

Portal path (approximate): User flow → **Applications** → Add application → select `box-web`.

If using policy-specific URLs (`p=B2C_1_signupsignin`), ASP.NET Core may need:

```json
"Auth:Entra": {
  "Authority": "https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0",
  "PolicyId": "B2C_1_signupsignin"
}
```

**Resolved:** the unified v2.0 authority works once the app is linked to the default user
flow; no `p=`/`PolicyId` is required. The one OIDC option that had to change is
`options.ResponseMode = OpenIdConnectResponseMode.Query` (see
[box-application-wiring.md](./box-application-wiring.md#response-mode-correlation)) — the
`form_post` default drops the `SameSite=Lax` correlation cookie against the cross-site CIAM
origin.

## Real Entra vs. Keycloak (handled by the `boe` stack)

This is automatic. The `boe` stack never wires Keycloak: `EntraProofOrchestrator` sets the
provider/tenant/callback paths, and the authority, client id, and client secret load from
Key Vault. See [box-application-wiring.md](./box-application-wiring.md). The effective
`users-api` configuration is:

```text
Auth__Provider=Entra
Auth__Entra__Authority=<Key Vault: auth-entra-authority>
Auth__Entra__TenantId=9af8af7b-10ee-4bd5-b71c-20daa8e37878
Auth__Entra__ClientId=<Key Vault: auth-entra-client-id>
Auth__Entra__ClientSecret=<Key Vault: auth-entra-client-secret>
Auth__Entra__PublicOrigin=<boe web origin, wired post-orchestrate>
```

## Manual test script

1. Browse to web app → Sign in.
2. Confirm redirect host is `rdbox.ciamlogin.com` (not Keycloak, not wrong tenant).
3. Create test user in External ID flow.
4. Confirm landing back on web app with avatar initials.
5. `GET /api/Auth/me` returns session; `GET /api/UserProfile/me` returns profile.

## Troubleshooting

| Error | Likely cause |
|-------|----------------|
| `invalid_request: redirect_uri` | Redirect URI not registered or `PublicOrigin` mismatch |
| Correlation failed | Cookie set on web origin but callback hits different origin |
| Missing oid claim | User flow token configuration |
| `AADB2C` Graph errors | Wrong API / CIAM tenant APIs need External ID permissions |
| Authority metadata 404 | Wrong authority URL (workforce URL used against CIAM tenant) |

## Difference from Keycloak emulation

| Aspect | Keycloak (local) | Entra External ID (proof) |
|--------|------------------|---------------------------|
| Authority | `http://localhost:.../realms/box` | `https://rdbox.ciamlogin.com/.../v2.0` |
| Client id/secret | `box-web` / `box-web-secret` | App registration + KV secret |
| Users | `test`/`test` in realm JSON | User flow sign-up |
| Social login | Manual Keycloak IdP config | External ID identity providers |
