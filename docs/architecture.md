# Architecture & box rules

This is the conceptual reference for how box is put together. Read it once before adding an
API, view, emulator, or stack; the step-by-step guides in [`guides/`](./guides/) assume it.

## The five layers

| Layer | Namespace prefix | Rule |
|-------|------------------|------|
| **box-bottom** | `BoxBottom.*` | Framework primitives. Must not know which stacks, APIs, or products exist. |
| **box-pack** | `BoxPack.*` | Composition + product customization. The only layer allowed to declare stacks. |
| **box-top** | `BoxTop.*` | The concrete host + shared platform services (edge, meta, users, web shell, AppHost). |
| **box-content** | `Foo.*`, `Bleeb.*`, `Blabber.*` | Business/demo APIs and their micro-frontends — "what a stack contains." |
| **box-test** | `BoxTest.*` | Unit + end-to-end tests. |

**Dependency direction:** `box-top` → `box-pack` → `box-content` → `box-bottom`. Never point
a lower layer at a higher one. If box-bottom needs to vary behavior, it exposes an extension
point (an options record, an anchor, a registry) that box-pack fills in.

## Stacks

A **stack** is one complete vertical slice — a Vue `web`, a YARP `edge`, a `meta` catalog
API, and one or more business APIs — that all share a name prefix (`box`, `bob`, `boe`).
Stacks reuse the *same project code*; they differ only in configuration.

- The stack name becomes the prefix on every Aspire resource: logical `users-api` in stack
  `bob` → resource `bob-users-api` (`ApiProjectOptions.StackName`).
- A stack is a `StackDefinition` (`src/box-bottom/BoxBottom.Aspire.Orchestration/StackDefinition.cs`).
  Assigning its `Name` re-prefixes every project; the copy constructor deliberately drops
  `EnvironmentVariables` so `stack with { Name = "bob" }` starts each project with a clean
  env dictionary and per-stack overrides never leak.
- Stacks are declared **only** in
  `src/box-pack/BoxPack.Aspire.Orchestration/Orchestrator.cs` (`Orchestrator.Orchestrate`),
  orchestrated by `StackOperations` (`src/box-bottom/BoxBottom.Aspire.Orchestration/StackOperations.cs`),
  and launched from `src/box-top/BoxTop.Aspire/AppHost.cs`.

`OrchestrateStack` creates resources in a fixed order — APIs first, then web, then edge
(which references the APIs), then meta, then cross-wired endpoint env vars — and throws if a
stack has zero APIs.

## Edge routing (YARP)

The edge is a reverse proxy configured entirely through environment variables. Routing is
derived automatically from a stack's APIs by
`src/box-bottom/BoxBottom.Aspire.Orchestration/EdgeRoutingConfiguration.cs`:

- The **primary API** (logical name `primary-api`) or the sole API in a single-API stack gets
  the catch-all `/api/{**catch-all}` with **no path transform** — the backend sees the path
  unchanged.
- Any **additional** API gets a prefixed route: the trailing `-api` is stripped, so
  `users-api` is exposed at `/api/users/{**catch-all}`. Two transforms fire — `PathRemovePrefix`
  strips `/api/users`, then `PathPrefix` re-adds `/api` — so the backend still sees `/api/...`.
- The `meta` API is routed separately at `/api/meta/{**catch-all}`.

**Rule:** name the stack's front-door API `primary-api` if you want it on the unprefixed
catch-all; everything else is reached at `/api/{name}/...`.

## API-to-API calls (Dapr)

Cross-API calls go through **Dapr service invocation**, not the edge. Every API is registered
with a Dapr sidecar whose app id is its prefixed `StackName`
(`DistributedApplicationBuilderExtensions.AddApiProject`). `ApiProjectOptions.ApiReferences`
records the allowed callee logical names. A gRPC-only backend sets
`GrpcOnlyAppChannel = true`, which drops its HTTP endpoint and configures the Dapr app
channel for gRPC (see `secondary-api`).

## Emulation framework

Emulation decouples **where a dependency is declared** (the consuming API) from **where it's
overridden** (the AppHost). It is two-phase and disabled entirely in Production.

- **Anchors** — assembly-level `[assembly: EmulationAnchor(name, typeof(IService), kind)]`
  attributes declaring "this keyed service is emulatable." Declared next to the code that
  uses the service. The real implementation must be registered as a **keyed** service under
  the same anchor name.
- **Orchestration phase (build time)** — a config-builder implementing
  `IEmulationConfigurationBuilder<...>` optionally spins up a support resource (an emulator
  API, Azurite, Keycloak) and serializes an emulation "document" into the consuming API's
  environment variables under its `EmulationKey`. Entry point: `ApiProjectOptions.WithEmulation`
  (and the wrappers `WithAzureTableEmulation`, `WithEntraEmulation`).
- **Startup phase (runtime)** — the API calls
  `AddEmulation(...).RegisterXxxEmulation().ApplyConfiguredEmulation()`. `EmulationRegistryApplier`
  reads each registered key's JSON from configuration and swaps the keyed services for
  emulator-backed ones via `EmulationRemapper`. In Production `AddEmulation` returns a no-op
  builder, so emulation cannot happen.

Emulator support resources are created once regardless of how many APIs reference them
(`EmulationOrchestrationState`). Diagnostics are exposed per API by
`EmulationDiagnosticController` and federated across a stack by the meta API.

## Web (Vue) composition

The web shell (`src/box-top/BoxTop.Web`) is deliberately thin: `main.js` installs the router
and web basics; `App.vue` creates and provides the tab, user-menu, and component-host
registries; `vite.config.js` proxies `/api` to the Aspire edge. **All API access is relative
`/api/...` fetches** — never hard-code an origin.

- A **view** is a `TabDefinition` (`id`, `label`, `type`, `route`, `load`, `visibility`,
  `enabled`) plus a Vue component, exported from a package's `src/index.js`.
- Feature views live in **box-content** packages (`@foo/*`); shared primitives/registries
  live in **box-bottom** (`@box-bottom/web-components`); product composition (navigation,
  branding, users) lives in **box-pack**.
- Tabs are aggregated in `src/box-pack/BoxPack.Web.Navigation/src/initialTabs.js`; routes are
  derived automatically in `router.js`.
- `visibility: requiresApiPredicate('primary-api')` hides a tab when its backend API isn't
  present in the current stack.
- Content web packages are linked by pnpm `file:` dependencies (not vite aliases), so adding
  one requires a `pnpm install` but no `vite.config.js` change.

## Configuration & conventions

- **Config precedence:** `appsettings.json` < `appsettings.Development.json` < environment
  variables. Aspire injects per-stack overrides as environment variables (`Section__Key`
  double-underscore nesting).
- **Secrets:** never commit secrets. Deployed/`boe` secrets come from Key Vault via
  `KeyVault:VaultUri` (see the [Entra reference](./ai/entra-implementation/keyvault.md));
  emulated stacks set the URI empty and stay offline.
- **Service defaults:** every API calls `builder.AddServiceDefaults()` and
  `app.MapDefaultEndpoints()` (health, telemetry, service discovery) from
  `BoxBottom.Aspire.ServiceDefaults`.
- **Solution membership:** add new projects to `box.slnx`. C#/`.csproj` are plain `<Project>`
  entries; Vue/`.esproj` entries include `<Build />` and `<Deploy />`.

## Key file index

| Concern | File |
|---------|------|
| Stack definition | `src/box-bottom/BoxBottom.Aspire.Orchestration/StackDefinition.cs` |
| Orchestration engine | `src/box-bottom/BoxBottom.Aspire.Orchestration/StackOperations.cs` |
| Resource creation | `src/box-bottom/BoxBottom.Aspire.Orchestration/DistributedApplicationBuilderExtensions.cs` |
| Edge routing rules | `src/box-bottom/BoxBottom.Aspire.Orchestration/EdgeRoutingConfiguration.cs` |
| API options | `src/box-bottom/BoxBottom.Aspire.Orchestration/ApiProjectOptions.cs` |
| Stack declaration (composition root) | `src/box-pack/BoxPack.Aspire.Orchestration/Orchestrator.cs` |
| AppHost entrypoint | `src/box-top/BoxTop.Aspire/AppHost.cs` |
| Emulation engine | `src/box-bottom/BoxBottom.Emulation/` |
| Emulation orchestration bridge | `src/box-bottom/BoxBottom.Aspire.Orchestration/EmulationOrchestrationExtensions.cs` |
| Web navigation / tabs | `src/box-pack/BoxPack.Web.Navigation/src/` |
