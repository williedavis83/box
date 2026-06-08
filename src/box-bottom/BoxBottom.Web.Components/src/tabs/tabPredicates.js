/**
 * @param {import('./tabTypes.js').TabDefinition} tab
 * @param {import('./tabTypes.js').TabContext} ctx
 */
export function isTabVisible(tab, ctx) {
  return tab.visibility?.(ctx) ?? true
}

/**
 * @param {import('./tabTypes.js').TabDefinition} tab
 * @param {import('./tabTypes.js').TabContext} ctx
 */
export function isTabEnabled(tab, ctx) {
  if (!isTabVisible(tab, ctx)) {
    return false
  }

  return tab.enabled?.(ctx) ?? true
}

/**
 * @param {import('./tabTypes.js').TabDefinition[]} tabs
 * @param {import('./tabTypes.js').TabContext} ctx
 */
export function filterVisibleTabs(tabs, ctx) {
  return tabs.filter((tab) => isTabVisible(tab, ctx))
}

/**
 * @param {import('./tabTypes.js').TabDefinition[]} tabs
 * @param {import('./tabTypes.js').TabContext} ctx
 */
export function filterEnabledTabs(tabs, ctx) {
  return tabs.filter((tab) => isTabEnabled(tab, ctx))
}
