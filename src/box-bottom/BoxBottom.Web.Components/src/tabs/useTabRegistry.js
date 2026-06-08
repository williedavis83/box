import { computed, inject, ref, shallowRef } from 'vue'
import { filterEnabledTabs, filterVisibleTabs } from './tabPredicates.js'

export const TAB_REGISTRY_KEY = Symbol('tabRegistry')
export const REGISTER_DYNAMIC_TAB_KEY = Symbol('registerDynamicTab')

/**
 * @param {import('./tabTypes.js').TabDefinition[]} initialTabs
 * @param {import('./tabTypes.js').TabContext} [initialContext]
 */
export function createTabRegistry(initialTabs = [], initialContext = {}) {
  const tabs = ref([...initialTabs])
  const context = shallowRef({ ...initialContext })

  const visibleTabs = computed(() => filterVisibleTabs(tabs.value, context.value))
  const enabledTabs = computed(() => filterEnabledTabs(tabs.value, context.value))

  /**
   * @param {import('./tabTypes.js').TabDefinition} tab
   */
  function addTab(tab) {
    if (tabs.value.some((existing) => existing.id === tab.id)) {
      return
    }

    tabs.value = [...tabs.value, tab]
  }

  /**
   * @param {string} id
   */
  function removeTab(id) {
    tabs.value = tabs.value.filter((tab) => tab.id !== id)
  }

  /**
   * @param {import('./tabTypes.js').TabContext} nextContext
   */
  function setContext(nextContext) {
    context.value = { ...nextContext }
  }

  return {
    tabs,
    context,
    visibleTabs,
    enabledTabs,
    addTab,
    removeTab,
    setContext,
  }
}

export function useTabRegistry() {
  const registry = inject(TAB_REGISTRY_KEY)

  if (!registry) {
    throw new Error('useTabRegistry must be used within a tab registry provider')
  }

  return registry
}
