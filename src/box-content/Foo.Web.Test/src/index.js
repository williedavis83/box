export { default as TestTabPanel } from './TestTabPanel.vue'

/** @type {import('@box-bottom/web-components').TabDefinition} */
export const testTab = {
  id: 'test',
  label: 'Test',
  type: 'link',
  route: '/test',
  load: () => import('./TestTabPanel.vue'),
  visibility: () => !import.meta.env.PROD,
  enabled: () => true,
}
