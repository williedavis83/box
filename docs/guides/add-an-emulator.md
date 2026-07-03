# Add a new emulator

This makes a dependency emulatable so a stack can run offline. Read the **Emulation
framework** section of [../architecture.md](../architecture.md) first — the terms (anchor,
config-builder, registry, remapper) are used below.

Two reference shapes exist; copy whichever fits:

- **In-DI replacement** — swap a keyed service for an in-process fake. Model:
  `src/box-bottom/BoxBottom.AzureTable.Emulation/` + `BoxBottom.AzureTable.Aspire/`.
- **External emulator service** — run a small emulator API and point the real client at it.
  Model: `Blabber.Emulator` + `src/box-content/Bleeb.Emulator.Api/`.

Example: emulate an `IPaymentGateway`.

## 1. Declare anchors in the consuming API

Next to the code that uses the service, declare an assembly-level anchor, and register the
real implementation as a **keyed** service under the same name:

```csharp
[assembly: EmulationAnchor("payments:primary", typeof(IPaymentGateway), EmulationAnchorKind.Singleton)]
// ...
services.AddKeyedSingleton<IPaymentGateway, RealPaymentGateway>("payments:primary");
```

`Kind` can be `Singleton`, `List`, or `Dictionary`.

## 2. Create a config document + config-builder

Implement `IEmulationConfigurationBuilder<TConfig, THostedConfig>`
(`src/box-bottom/BoxBottom.Emulation.Shared/`). Model on
`AzureTableEmulationConfigurationBuilder`. It needs:

- a unique `EmulationKey` (e.g. `"Payments_Emulation"`),
- `Override...` methods that populate the document's `Singletons`/`Lists`/`Dictionaries`,
- `BuildDocument()`,
- `OrchestrateEmulationResources(orchestrator)` — call
  `orchestrator.OrchestrateSupportProject(...)` **only** if you need an out-of-process
  emulator (external-service shape); leave empty for pure in-DI replacement.

## 3. Write the runtime applier

Implement `IEmulationRegistryEntryApplier` (model:
`AzureTableEmulationDocumentApplier`). Deserialize the document and use
`EmulationRemapper<TConfig>` / `EmulationKeyedServiceReplacement.ReplaceKeyed` to swap each
keyed service for an emulator-backed one. Add a hosted-service factory too if the emulator
needs startup seeding (see `BleebEmulatorSeedHostedService`).

## 4. Add registry extensions

Add `RegisterPaymentsEmulation(this IEmulationRegistry)` that calls `registry.Register(...)`
with your key, service factory, optional hosted-service factory, and applier. Also add the
`IEmulationBuilder` overload used from `Program.cs` (no-op in Production). Model:
`AzureTableEmulationRegistryExtensions`.

## 5. (Optional) add an Aspire wrapper

For ergonomics, add
`WithPaymentsEmulation(this ApiProjectOptions, StackOperations, Action<Builder>)` that
orchestrates any support/container resource, then calls
`options.WithEmulation(stackOperations, builder)`. Model: `WithAzureTableEmulation`.

## 6. Wire it up

Runtime — in the consuming API's `Program.cs`, add to the emulation chain:

```csharp
builder.AddEmulation(typeof(PaymentsController).Assembly, /* ...other assemblies... */)
    .RegisterPaymentsEmulation()
    .ApplyConfiguredEmulation();
```

Orchestration — in `src/box-pack/BoxPack.Aspire.Orchestration/Orchestrator.cs`, apply the
override to the stacks that should emulate it:

```csharp
boxStack["orders-api"].WithPaymentsEmulation(
    stackOperations, b => b.OverrideSingleton("payments:primary"));
```

## Safety & verification

- Emulation is automatically disabled in Production (`AddEmulation` returns a no-op builder),
  and JSON is only injected when a `WithEmulation` override is applied — a stack with no
  override runs the real service.
- The `EmulationDiagnosticController` (per API) and the meta API's federated diagnostics show
  which anchors are active; the web **Emulation** tab surfaces this.
- Add unit tests under `src/box-test/BoxTest.Aspire.Orchestration.Test` (model:
  `EmulationRemapperTests`, `AzureTableEmulationStartupTests`).
