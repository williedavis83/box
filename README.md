# box

**box** (the "box-chassis") is an opinionated application chassis built on **.NET Aspire**
and **Vue 3**. It gives you a ready-made vertical slice — a Vue web shell, a YARP edge
proxy, a meta/catalog API, pluggable authentication, and a first-class **emulation
framework** — that you compose into one or more named **stacks**. The same project code runs
as `box` (fully emulated, offline), `bob` (dev auth), or `boe` (real Microsoft Entra), just
by changing configuration.

The goals are: run the whole system locally with one command, keep cloud dependencies
emulated by default, and make "add another API / view / emulator / stack" a small, well-worn
path.

## Repository layout

Everything lives under `src/`, split into five layers. The dependency direction is
`box-top` → `box-pack` → `box-content` → `box-bottom`, with `box-test` referencing
everything.

| Layer | Path | Responsibility |
|-------|------|----------------|
| **box-bottom** | `src/box-bottom/` | Reusable framework primitives: the Aspire orchestration engine, the emulation engine, concrete emulators/adapters (Azure Table, Auth), Dapr helpers, and shared Vue components. Nothing here knows which stacks exist. |
| **box-pack** | `src/box-pack/` | Composition + product customization. `BoxPack.Aspire.Orchestration/Orchestrator.cs` is the one place stacks are declared and wired. Also product web packages (navigation, branding, users). |
| **box-top** | `src/box-top/` | The concrete deployable host + shared platform services: `BoxTop.Aspire` (AppHost), `BoxTop.Edge` (proxy), `BoxTop.Meta.Api`, `BoxTop.Users.Api`, Vue shells (`BoxTop.Web`, `BoxTop.Web.Bob`, …). |
| **box-content** | `src/box-content/` | Demo/business APIs and their Vue micro-frontends (`Foo.*`, `Bleeb.*`, `Blabber.*`). This is "the stuff a stack contains." |
| **box-test** | `src/box-test/` | `BoxTest.Aspire.Orchestration.Test` (xUnit) and `BoxTest.Playwright` (end-to-end, per stack). |

Solution file: `box.slnx`.

## Stacks

A **stack** is one complete vertical slice (web + edge + meta + APIs) sharing a name prefix.
All three below run from the same code with different configuration:

| Stack | Auth | Cloud needed? |
|-------|------|---------------|
| `box` | Keycloak emulation (emulated Entra) | No — default local dev |
| `bob` | ZeroAuth (dev JSON login) | No |
| `boe` | Real Entra External ID + Key Vault | Yes — `az login` |

## Prerequisites

- **.NET 10 SDK** (Aspire AppHost)
- **Node.js + pnpm** (Vue web packages)
- **Docker Desktop** (Keycloak, Azurite, and other containers)
- **Dapr CLI + runtime** (every API runs with a Dapr sidecar)
- **Azure CLI** — only for the `boe` stack (`az login` for Key Vault access)

## Quick start

```powershell
dotnet run --project src/box-top/BoxTop.Aspire
```

This launches the Aspire dashboard with the `box` and `bob` stacks (and `boe` if Azure is
reachable). Open the dashboard URL from the console, then open a stack's `web` endpoint.

To exercise real Entra on the `boe` stack, run `az login` first — see
[docs/ai/entra-implementation/README.md](docs/ai/entra-implementation/README.md).

## Documentation

| Doc | What it covers |
|-----|----------------|
| [docs/architecture.md](docs/architecture.md) | The box rules: layering, stacks, edge routing, emulation model, conventions |
| [docs/guides/add-an-api.md](docs/guides/add-an-api.md) | Add a new backend API to a stack |
| [docs/guides/add-a-view.md](docs/guides/add-a-view.md) | Add a new Vue view/tab to the web app |
| [docs/guides/add-an-emulator.md](docs/guides/add-an-emulator.md) | Emulate a new dependency |
| [docs/guides/add-a-stack.md](docs/guides/add-a-stack.md) | Add a new stack |
| [docs/ai/entra-implementation/](docs/ai/entra-implementation/) | Reference implementation: real Entra External ID via Terraform + Key Vault |

## Testing

```powershell
dotnet test src/box-test/BoxTest.Aspire.Orchestration.Test   # orchestration/emulation unit tests
```

Playwright end-to-end tests live in `src/box-test/BoxTest.Playwright` and run per stack
against the endpoints Aspire injects.
