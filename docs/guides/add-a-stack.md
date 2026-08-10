# Add a new stack

A **stack** is a full vertical slice (web + edge + meta + APIs) with its own name prefix.
Adding one is almost entirely done by cloning an existing `StackDefinition` and applying
per-stack configuration. See [../architecture.md](../architecture.md) for the model.

All edits are in `src/box-pack/BoxPack.Aspire.Orchestration/Orchestrator.cs`
(`Orchestrator.Orchestrate`). Example: a `moo` stack.

## 1. Clone an existing definition

Cloning re-prefixes every resource to `moo-*` and gives each project a **fresh empty
environment dictionary**, so overrides never leak from the source stack:

```csharp
private const string _mooStackName = "moo";
// ...
var mooStack = boxStack with { Name = _mooStackName };
```

Add or remove APIs on the clone if `moo` should differ from `box`:

```csharp
mooStack.AddApi(new ApiProjectOptions(
    logicalName: "orders-api",
    projectPath: @"..\..\box-content\Orders.Api\Orders.Api.csproj"));
```

A stack must have at least one API or orchestration throws.

## 2. Apply per-stack configuration

Index APIs by logical name and set environment variables, or attach emulation:

```csharp
mooStack["users-api"].EnvironmentVariables["Auth__Provider"] = ZeroAuthAuthProvider.Name;
mooStack["secondary-api"].EnvironmentVariables["World__Message"] = "Moo";
mooStack["primary-api"].WithAzureTableEmulation(stackOperations, /* ... */);
```

Stacks selecting ZeroAuth skip Entra Key Vault loading automatically. Entra emulators
should transmit their provider configuration through an emulation document instead of
overriding `Auth__Entra__*` environment variables directly.

### Optional: distinct frontend shell

`CreateStackDefinition` seeds every stack with the AppHost default web path
(`../BoxTop.Web`). To run a different Vue shell for this stack, override
`Web.ProjectPath` after cloning (path is relative to `BoxTop.Aspire`):

```csharp
mooStack.Web = mooStack.Web with { ProjectPath = "../BoxTop.Web.Moo" };
```

Clone an existing shell (same shape as `BoxTop.Web` / `BoxTop.Web.Bob`), point its
`package.json` at the right box-pack navigation/branding packages, add the `.esproj` to
`box.slnx`, and run `pnpm install` in the new shell. Edge/meta stay shared project paths;
only the Vite app folder changes. Example in-tree: `bob` → `../BoxTop.Web.Bob`.

## 3. Orchestrate it

Add it to the `stacks` dictionary **before** the shared-wiring calls, so
`AzuriteOrchestrator.WireToApis`, `BleebOrchestrator.WireToPrimaryApis`, Keycloak/Entra
wiring, etc. can find it:

```csharp
var stacks = new Dictionary<string, StackResources>(StringComparer.OrdinalIgnoreCase)
{
    [_boxStackName] = stackOperations.OrchestrateStack(boxStack),
    [_bobStackName] = stackOperations.OrchestrateStack(bobStack),
    [_mooStackName] = stackOperations.OrchestrateStack(mooStack),
};
```

## 4. Opt into shared support wiring

If `moo` needs a shared support resource, add its name to the relevant `WireTo...` call
(several accept `params string[] stackNames`). Skip this to keep the stack lean.

For **Keycloak Entra emulation**, call `users-api.WithEntraEmulation(stackOperations)` on the
stack definition, add a realm JSON under `BoxBottom.Auth.Aspire/keycloak/` (and register it
on `KeycloakOrchestrationConfiguration.ImportedRealms`), then bind the stack to that realm:

```csharp
KeycloakOrchestrator.WireEntraEmulationToUsersApi(
    stackOperations,
    stacks,
    new KeycloakStackBinding(_mooStackName, KeycloakOrchestrationConfiguration.GetRealm("moo")));
```

`PublicOrigin` is taken from the stack's **web** HTTP URL automatically. Do not point it at
the edge. Prefer a realm name tied to the stack (or an explicit `KeycloakStackBinding`) so
multiple Keycloak-backed stacks can share one Keycloak container with separated identity
spheres. Leave `bob` (ZeroAuth) and `boe` (real Entra) on their existing auth models unless
you intentionally convert them.

## What you get for free

Playwright endpoint env vars, the meta catalog, edge routes, and per-API Dapr sidecars are
generated automatically for whatever is in the `stacks` dictionary. No edits to the edge,
meta, web, or AppHost projects are required.

## Verify

- `dotnet run --project src/box-top/BoxTop.Aspire` and confirm `moo-*` resources appear and
  are healthy in the Aspire dashboard.
- Open the `moo` `web` endpoint and confirm auth/behavior matches the per-stack config.
- Add coverage in `src/box-test/BoxTest.Aspire.Orchestration.Test` (model:
  `StackPropertiesTests`, `AuthOrchestrationTests`).
