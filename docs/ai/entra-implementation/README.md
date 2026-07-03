# Entra implementation — proof plan for box-chassis



This folder documents what is required to **prove real Microsoft Entra works** in box-chassis (not Keycloak emulation), using the **RD-Box** Azure subscription. The long-term target is **customer-facing sign-in with social identity providers**; the near-term target is a minimal, repeatable Azure footprint managed by **Terraform** with secrets in **Key Vault**.



## Implementation status



| Component | Location |

|-----------|----------|

| Terraform (KV + app reg) | [`infra/entra-proof/`](../../../infra/entra-proof/) |

| Portal runbook | [`infra/entra-proof/README.md`](../../../infra/entra-proof/README.md) |

| **boe** Aspire stack (real Entra) | [`Orchestrator.cs`](../../../src/box-pack/BoxPack.Aspire.Orchestration/Orchestrator.cs), [`EntraProofOrchestrator.cs`](../../../src/box-bottom/BoxBottom.Auth.Aspire/EntraProofOrchestrator.cs) |

| Key Vault config in users-api | [`BoxTop.Users.Api/Program.cs`](../../../src/box-top/BoxTop.Users.Api/Program.cs) |

| Playwright proof | [`auth-entra-ui.spec.js`](../../../src/box-test/BoxTest.Playwright/tests/auth-entra-ui.spec.js) |

| CI | [`.github/workflows/ci.yml`](../../../.github/workflows/ci.yml) |



Enable the **boe** stack:



```powershell

$env:BOX_ENTRA_PROOF = "1"

$env:Auth__Entra__ClientId = "<terraform output entra_client_id>"

$env:KeyVault__VaultUri = "<terraform output key_vault_uri>"

dotnet run --project src/box-top/BoxTop.Aspire

```



## Documents



| Doc | Purpose |

|-----|---------|

| [azure-subscription-inventory.md](./azure-subscription-inventory.md) | What exists in Azure today (PowerShell/`az` interrogation, 2026-07-01) |

| [proof-criteria.md](./proof-criteria.md) | Definition of done for “Entra actually works” |

| [terraform.md](./terraform.md) | Infrastructure to provision with Terraform |

| [keyvault.md](./keyvault.md) | Secret naming, access model, and runtime binding |

| [entra-external-id-setup.md](./entra-external-id-setup.md) | App registration, authority URLs, redirect URIs, user flows |

| [box-application-wiring.md](./box-application-wiring.md) | Mapping Azure resources → `Auth:Entra` configuration in box |

| [social-login-roadmap.md](./social-login-roadmap.md) | Path from workforce/CIAM email login → full social IdPs |



## Stack layout



| Stack | Auth | Notes |

|-------|------|-------|

| **box** | Keycloak emulation | Default local dev |

| **bob** | ZeroAuth | Dev JSON login |

| **boe** | Real Entra External ID | Gated by `BOX_ENTRA_PROOF=1` |



## Related code



| Area | Location |

|------|----------|

| OIDC + cookie auth | `src/box-bottom/BoxBottom.Auth.Entra/` |

| Entra options | `src/box-bottom/BoxBottom.Auth.Contract/EntraAuthOptions.cs` |

| Keycloak emulation (box) | `src/box-bottom/BoxBottom.Auth.Aspire/KeycloakOrchestrator.cs` |

| Real Entra proof (boe) | `src/box-bottom/BoxBottom.Auth.Aspire/EntraProofOrchestrator.cs` |

| Default CIAM config | `src/box-top/BoxTop.Users.Api/appsettings.json` |

| Claims → external user | `src/box-bottom/BoxBottom.Auth.Entra/EntraClaimsMapper.cs` |

