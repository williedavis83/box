# Proof criteria — “Entra actually works”

## Scope

Prove that box-chassis **production-style Entra authentication** works end-to-end:

1. User opens the **web app** on a **stable HTTPS origin**.
2. User clicks **Sign in** → redirected to **Microsoft Entra External ID** (not Keycloak).
3. User completes sign-in (initially: **local email account** in External ID user flow).
4. Browser returns to `{PublicOrigin}/api/users/signin-oidc` with a valid OIDC code exchange.
5. `users-api` creates/links **external user** + **user profile**, sets **`box.auth`** cookie.
6. SPA shows authenticated session (`/api/Auth/me`, user menu, profile page).

Keycloak emulation remains valid for **local Aspire**; this proof uses **real Azure Entra External ID**.

## Out of scope (phase 1)

- Google / Facebook / Apple social IdPs (documented in [social-login-roadmap.md](./social-login-roadmap.md))
- Multi-stack production deployment (single “proof” environment is enough)
- Custom domains on External ID (`login.customers.example.com`)
- MFA conditional access policies
- B2B guest users from other tenants as the primary customer identity model

## Must pass

### Infrastructure

- [x] Terraform applies cleanly to RD-Box subscription (idempotent second apply).
- [x] Key Vault exists; client secret stored; no secret committed to git.
- [x] App registration + enterprise app exist with correct redirect URIs (v2 access tokens).
- [x] External ID **user flow** allows sign-up/sign-in with email (local account).

### Application

- [x] `Auth__Provider=Entra` for proof `users-api` (**boe** stack via `EntraProofOrchestrator`).
- [x] `Auth__Entra__Authority` set to **ciamlogin.com** authority (default in `appsettings.json` + orchestration).
- [x] `Auth__Entra__PublicOrigin` matches the browser-facing web URL (`EntraProofOrchestrator.WireRealEntraToUsersApi`).
- [x] `Auth__Entra__ExternalCallbackPath=/api/users/signin-oidc` (edge path; unchanged from local).
- [x] Keycloak env overrides **not** applied to **boe** (only **box** uses Keycloak).
- [x] Correlation cookie / callback origin verified against real Entra (fixed via `response_mode=query`).

### Functional

- [x] Manual login: new External ID user → profile created → `/api/UserProfile/me` returns data.
- [ ] Repeat login: same `externalId` maps to same internal `userId`.
- [ ] Logout clears session; protected endpoints return 401.
- [x] **boe** stack **rejects** ZeroAuth when Entra is configured (`auth-api.spec.js`).

### Automated

- [x] Playwright test: **boe** web → Sign in → External ID redirect (`auth-entra-ui.spec.js`; full login when `ENTRA_PROOF_*` set).
- [x] CI job runs Terraform validate + unit tests (`.github/workflows/ci.yml`).

## Evidence to capture

| Artifact | Description |
|----------|-------------|
| Terraform plan/apply output | Shows Key Vault + app registration resources |
| Key Vault secret names (not values) | e.g. `auth-entra-client-secret` |
| App registration screenshot / export | Redirect URIs, granted API permissions |
| OIDC metadata URL | `{authority}/.well-known/openid-configuration` returns 200 |
| HAR or server log snippet | Successful `/api/users/signin-oidc` callback |
| Playwright trace | **boe** Entra login spec green (`auth-entra-ui.spec.js`) |

## Failure modes to explicitly test

| Scenario | Expected |
|----------|----------|
| Wrong `redirect_uri` | Entra error page; no partial session |
| Missing client secret | 500 at token exchange; no cookie |
| `PublicOrigin` = edge instead of web | Correlation failed (same as Keycloak bug) |
| Claims missing `oid` | 500 with clear log; mapper requires object id |
| ZeroAuth against Entra-configured **boe** | HTTP 403 |

## Success statement

> After `terraform apply` and `az login`, a developer starts Aspire, signs in through
> **Entra External ID** on the **boe** stack, and uses the web app authenticated — with
> Playwright verifying the flow. No per-run environment variables are required.
