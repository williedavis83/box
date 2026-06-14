export { default as BlabberView } from './BlabberView.vue'
export { fetchAllBlabberData } from './blabberApi.js'
import { requiresApiPredicate } from '@box-bottom/web-components'

/** @type {import('@box-bottom/web-components').TabDefinition} */
export const blabberTab = {
  id: 'blabber',
  label: 'Blabber',
  type: 'link',
  route: '/blabber',
  load: () => import('./BlabberView.vue'),
  visibility: requiresApiPredicate('primary-api'),
  enabled: requiresApiPredicate('primary-api'),
}
