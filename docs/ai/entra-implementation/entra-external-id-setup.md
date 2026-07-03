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
| Local Vite (real Entra dev test) | `http://localhost:{port}` | `http://localhost:{port}/api/users/signin-oidc` |
| Proof/staging host | `https://{host}` | `https://{host}/api/users/signin-oidc` |

Add **both** if testing locally against real Entra without Keycloak.

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

Evaluate during implementation whether External ID requires `Policy` on OIDC options (`options.ResponseMode`, custom `OnRedirectToIdentityProvider` to append `p=`). Many CIAM tenants work with the unified v2.0 authority once the app is linked to the default user flow — **verify with a live authorize URL**.

## Disable Keycloak for real Entra proof

In Aspire / deployment for proof environment:

- Do **not** call `KeycloakOrchestrator.WireEntraEmulationToUsersApi` for the target stack, **or**
- Override env vars after wiring:

```text
Auth__Entra__Authority=https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0
Auth__Entra__TenantId=9af8af7b-10ee-4bd5-b71c-20daa8e37878
Auth__Entra__ClientId=<from app registration>
Auth__Entra__ClientSecret=<from Key Vault>
Auth__Entra__PublicOrigin=<browser web origin>
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
