import { createRouter, createWebHistory } from 'vue-router'
import { initialTabs } from '@box-pack/web-navigation'

/**
 * @param {import('@box-bottom/web-components').TabDefinition} tab
 */
function tabToRoute(tab) {
  return {
    path: tab.route,
    name: tab.id,
    component: () => tab.load().then((module) => module.default),
    meta: tab.meta ?? {},
  }
}

const linkTabs = initialTabs.filter((tab) => tab.type === 'link')

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/hello' },
    ...linkTabs.map(tabToRoute),
  ],
})

/**
 * @param {import('@box-bottom/web-components').TabDefinition} tab
 */
export function addRouteForTab(tab) {
  if (tab.type !== 'link' || router.hasRoute(tab.id)) {
    return
  }

  router.addRoute(tabToRoute(tab))
}
