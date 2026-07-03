# Add a new API

This adds a new backend API and exposes it through the edge in one or more stacks. See
[../architecture.md](../architecture.md) for the concepts (edge routing, Dapr, service
defaults).

Example: an `orders-api` that also calls the existing `secondary-api`.

## 1. Create the project

Create the project under `src/box-content/` (business APIs) or `src/box-top/` (shared
platform APIs). Model it on `src/box-content/Foo.Primary.Api`.

Minimum `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();          // health, telemetry, service discovery
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
app.MapControllers();
app.MapDefaultEndpoints();              // /health, etc.
app.Run();
```

Reference `BoxBottom.Aspire.ServiceDefaults` so `AddServiceDefaults()` is available.

## 2. Add it to `box.slnx`

Add a `<Project Path="src/box-content/Orders.Api/Orders.Api.csproj" />` entry under the
`/src/box-content/` folder.

## 3. Register it in a stack

In `src/box-pack/BoxPack.Aspire.Orchestration/Orchestrator.cs`, add it to the stack
definition with `AddApi`:

```csharp
boxStack.AddApi(new ApiProjectOptions(
    logicalName: "orders-api",
    projectPath: @"..\..\box-content\Orders.Api\Orders.Api.csproj",
    apiReferences: ["secondary-api"]));   // Dapr callees, if any
```

- `logicalName` drives the edge route, meta catalog key, and Playwright env var names.
- `projectPath` is relative to the AppHost (`BoxTop.Aspire`).
- For a gRPC-only backend with no HTTP endpoint, pass `grpcOnlyAppChannel: true` (see
  `secondary-api`).

Because the stack now has more than one API and the name isn't `primary-api`, the edge
exposes it automatically at **`/api/orders/{**catch-all}`** (the `-api` suffix is stripped).
To put it on the unprefixed `/api/...` catch-all instead, name it `primary-api`.

## 4. (Optional) per-stack configuration

Override configuration for a specific stack via the API's environment variables:

```csharp
bobStack["orders-api"].EnvironmentVariables["Orders__Mode"] = "Demo";
```

## 5. (Optional) attach emulation or shared dependencies

If the API depends on something emulatable, attach it here (see
[add-an-emulator.md](./add-an-emulator.md)):

```csharp
boxStack["orders-api"].WithAzureTableEmulation(stackOperations, ...);
```

If it needs an existing shared support resource (Azurite, Bleeb, Keycloak), add its logical
name to the relevant `WireTo...` call near the bottom of `Orchestrate`.

## What you get for free

Once registered, the API is automatically: routed by the edge, added to the meta catalog,
given a Dapr sidecar + `/health` check, and wired into Playwright endpoint env vars. No
changes to the edge, meta, or web projects are required.

## Verify

- `dotnet run --project src/box-top/BoxTop.Aspire`, open the Aspire dashboard, confirm the
  `{stack}-orders-api` resource is healthy.
- Hit `{stack-web-or-edge}/api/orders/...` and confirm the response.
- Consider adding coverage in `src/box-test/BoxTest.Aspire.Orchestration.Test`.
