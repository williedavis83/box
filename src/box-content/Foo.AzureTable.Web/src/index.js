export { default as AzureTableView } from './AzureTableView.vue'
export {
  ensureAzureTable,
  fetchAzureTableEntities,
  fetchAzureTables,
  fetchAzureTableStatus,
  upsertAzureTableEntity,
} from './azureTableApi.js'
import { requiresApiPredicate } from '@box-bottom/web-components'

/** @type {import('@box-bottom/web-components').TabDefinition} */
export const azureTableTab = {
  id: 'azure-table',
  label: 'Azure Table',
  type: 'link',
  route: '/azure-table',
  load: () => import('./AzureTableView.vue'),
  visibility: requiresApiPredicate('primary-api'),
  enabled: requiresApiPredicate('primary-api'),
}
