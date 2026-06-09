export { default as HelloView } from './HelloView.vue'
import { requiresApiPredicate } from '@box-bottom/web-components'

/** @type {import('@box-bottom/web-components').TabDefinition} */
export const helloTab = {
  id: 'hello',
  label: 'Hello',
  type: 'link',
  route: '/hello',
  load: () => import('./HelloView.vue'),
  visibility: requiresApiPredicate('primary-api'),
  enabled: requiresApiPredicate('primary-api'),
}
