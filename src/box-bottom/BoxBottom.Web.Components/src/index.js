export { default as BrandingComponent } from './components/BrandingComponent.vue'
export { default as Header } from './components/Header.vue'
export { default as Footer } from './components/Footer.vue'
export { default as Main } from './components/Main.vue'
export { default as TabMenu } from './components/TabMenu.vue'
export { default as LinkTab } from './components/LinkTab.vue'
export { default as MenuTab } from './components/MenuTab.vue'
export { default as HeaderPlaceholder } from './components/HeaderPlaceholder.vue'

export { createUserMenuRegistry, useUserMenuRegistry, USER_MENU_REGISTRY_KEY } from './userMenu/useUserMenuRegistry.js'

export { createTabRegistry, useTabRegistry, TAB_REGISTRY_KEY, REGISTER_DYNAMIC_TAB_KEY } from './tabs/useTabRegistry.js'
export { isTabVisible, isTabEnabled, filterVisibleTabs, filterEnabledTabs } from './tabs/tabPredicates.js'
export { splitTabsForOverflow } from './tabs/tabOverflow.js'
export {
  createComponentHost,
  fetchMetaApiCatalog,
  loadComponentHost,
  requiresApiPredicate,
} from './host/componentHost.js'
export { useComponentHost, COMPONENT_HOST_KEY } from './host/useComponentHost.js'
