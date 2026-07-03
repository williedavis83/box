# Add a new view

This adds a new Vue view/tab to the web app. See [../architecture.md](../architecture.md)
for the web composition model.

A **view** is a `TabDefinition` + a Vue component, exported from a package's `src/index.js`.
Feature views live in their own **box-content** package and are registered once in
**box-pack** navigation. The web shell (`BoxTop.Web`) needs no changes.

Example: a `Widgets` view backed by `primary-api`.

## 1. Create a content web package

Model it on `src/box-content/Foo.AzureTable.Web`. Create
`src/box-content/Foo.Widgets.Web/` with:

`package.json`:

```json
{
  "name": "@foo/widgets-web",
  "version": "0.0.0",
  "private": true,
  "type": "module",
  "exports": { ".": "./src/index.js" },
  "peerDependencies": {
    "vue": "^3.5.0",
    "@box-bottom/web-components": "^0.0.0"
  }
}
```

`src/WidgetsView.vue` — your Vue component. Call the backend with **relative** fetches
(`fetch('/api/widgets/...')` or `/api/...` for the primary API); Vite proxies `/api` to the
edge. Put API calls in a sibling `src/widgetsApi.js` for testability (see `azureTableApi.js`).

`src/index.js` — export the component and the tab:

```js
export { default as WidgetsView } from './WidgetsView.vue'
import { requiresApiPredicate } from '@box-bottom/web-components'

/** @type {import('@box-bottom/web-components').TabDefinition} */
export const widgetsTab = {
  id: 'widgets',
  label: 'Widgets',
  type: 'link',
  route: '/widgets',
  load: () => import('./WidgetsView.vue'),
  visibility: requiresApiPredicate('primary-api'),
  enabled: requiresApiPredicate('primary-api'),
}
```

`requiresApiPredicate('...')` hides/disables the tab when that API isn't present in the
current stack. Omit it for an always-visible tab.

Add a matching `.esproj` (copy `Foo.AzureTable.Web.esproj`) so it participates in the build.

## 2. Register the package in navigation

In `src/box-pack/BoxPack.Web.Navigation/package.json`, add a `file:` dependency:

```json
"@foo/widgets-web": "file:../../box-content/Foo.Widgets.Web"
```

In `src/box-pack/BoxPack.Web.Navigation/src/initialTabs.js`, import and include the tab:

```js
import { widgetsTab } from '@foo/widgets-web'
// ...
export const initialTabs = [helloTab, blabberTab, azureTableTab, widgetsTab, testTab, emulationTab]
export { /* ...existing..., */ widgetsTab }
```

Routes are derived automatically from `initialTabs` in `router.js` — no router edit needed
for a normal `type: 'link'` tab. (Route-scoped tabs like the user profile are the exception;
they're added in `App.vue`.)

## 3. Add to the solution and install

- Add the `.esproj` to `box.slnx` under `/src/box-content/` with `<Build />` and `<Deploy />`.
- Run `pnpm install` so the new `file:` link resolves. No `vite.config.js` change is needed —
  content packages resolve via pnpm links, not vite aliases.

## Verify

- `dotnet run --project src/box-top/BoxTop.Aspire`, open a stack's `web` endpoint.
- Confirm the **Widgets** tab appears (only on stacks that have `primary-api`, if you gated
  it) and that navigating to `/widgets` loads the view and its data.
- Add a Playwright test under `src/box-test/BoxTest.Playwright/tests` if the view is
  user-facing.
