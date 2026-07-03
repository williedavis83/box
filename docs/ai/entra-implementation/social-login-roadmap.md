# Social login roadmap

Long-term goal: **customer sign-in via social identity providers** (Google, Microsoft personal, Facebook, Apple, etc.) on the same box-chassis auth stack, backed by **Entra External ID** (CIAM).

The RD-Box tenant type (**CIAM**) is the correct Microsoft product for this; workforce Entra ID is not the target architecture for consumer social login.

## Phased approach

### Phase 0 — Today (complete)

- OIDC middleware + cookie session in `BoxBottom.Auth.Entra`
- External user → profile linkage in Azure Table
- Keycloak emulates Entra for local Aspire
- ZeroAuth for bob dev stack

### Phase 1 — Prove real Entra (current initiative)

- Terraform + Key Vault + app registration
- External ID **email/password** user flow
- Hosted proof environment + Playwright E2E

**Exit criteria:** [proof-criteria.md](./proof-criteria.md)

### Phase 2 — First social IdP (recommend Google)

1. **External ID portal:** User flow → Identity providers → Add **Google**
2. Store Google OAuth client secret in Key Vault (`entra-idp-google-client-secret`)
3. Configure Google Cloud Console redirect URI per Microsoft docs (External ID callback URL)
4. Enable Google on the same sign-up/sign-in user flow assigned to `box-web`
5. Test: new user via “Sign in with Google” → `oid` claim → profile created

No box code change expected if OIDC claims remain `oid`, `name`, `email`. Re-test `EntraClaimsMapper` with Google-issued tokens.

### Phase 3 — Additional IdPs

| Provider | External ID support | Notes |
|----------|---------------------|-------|
| Microsoft (consumer) | Built-in | Useful for personal Microsoft accounts |
| Facebook | Built-in | App review / privacy policy |
| Apple | Built-in | Required for iOS consumer apps if applicable |
| GitHub | Custom OIDC | Optional for dev-heavy products |
| Enterprise SAML | Separate B2B path | Not social; different story |

Each IdP:

1. Register app with provider
2. Store secrets in Key Vault
3. Configure in External ID
4. Add to user flow
5. Playwright smoke test per IdP (manual or recorded)

### Phase 4 — UX and account linking

- **Account linking:** same email across providers → Microsoft linking rules / custom policy (evaluate External ID **custom authentication extensions** if needed)
- **Branding:** custom domain for `ciamlogin.com`, company logo, email templates
- **Token enrichment:** optional Graph calls post-sign-in (avoid in hot path initially)
- **Provider label in `ExternalUser`:** extend beyond `"AzureAd"` to `{ "EntraExternalId", idp: "google" }` if multiple IdPs require disambiguation

### Phase 5 — Operations

- Terraform / Graph automation for IdP configs where portal clicks don't scale
- Secret rotation runbooks (Key Vault + IdP consoles)
- Conditional Access / MFA for high-risk actions (not necessarily at first login)
- Rate limiting on `Auth/login` and ZeroAuth endpoints

## Architecture (target)

```text
┌─────────────┐     OIDC      ┌──────────────────────────┐
│  BoxTop.Web │ ◄───────────► │ Entra External ID (CIAM) │
│  + Edge     │               │  User flow:              │
└──────┬──────┘               │   - Email/password       │
       │                      │   - Google               │
       │                      │   - Facebook / Apple …   │
       ▼                      └──────────────────────────┘
┌─────────────┐
│ users-api   │──► Azure Table (externalUsers, userProfiles)
│ Entra OIDC  │
└─────────────┘
       ▲
       │ ClientSecret, IdP secrets
┌──────┴──────┐
│  Key Vault  │
└─────────────┘
       ▲
       │ Terraform
┌──────┴──────┐
│  infra/     │
└─────────────┘
```

## What box-chassis should *not* do

- Implement OAuth with Google/Facebook **directly** in ASP.NET (duplicates External ID)
- Store social tokens long-term in box (use session cookie only; `SaveTokens = false` today)
- Use workforce `login.microsoftonline.com` for consumer social

## Identity model considerations

| Topic | Recommendation |
|-------|----------------|
| Primary key | Entra `oid` per tenant (stable per user identity in CIAM) |
| Email collisions | Let External ID handle; monitor linking edge cases |
| Provider name | Start with `AzureAd`; later `EntraExternalId` + IdP metadata claim |
| Guest B2B users | Separate from consumer CIAM users unless product merges them |

## Microsoft documentation anchors

- [Entra External ID overview](https://learn.microsoft.com/en-us/entra/external-id/external-identities-overview)
- [Add identity providers to user flows](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-add-identity-providers)
- [OIDC in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/openid-connect)
- [Register apps in External ID](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-register-app)

## Decision log (to update during implementation)

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-07-01 | Use existing CIAM tenant (rdbox) | Already provisioned; aligned with social roadmap |
| 2026-07-01 | Terraform + Key Vault before social IdPs | Secrets and app registration must be solid first |
| TBD | First social IdP: Google | Widest test coverage, straightforward OAuth |
