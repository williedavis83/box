export { default as HelloView } from './HelloView.vue'

/** @type {import('@box-bottom/web-components').TabDefinition} */
export const helloTab = {
  id: 'hello',
  label: 'Hello',
  type: 'link',
  route: '/hello',
  load: () => import('./HelloView.vue'),
  visibility: () => true,
  enabled: () => true,
}
