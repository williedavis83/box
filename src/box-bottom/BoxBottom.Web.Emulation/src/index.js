export { default as EmulationDiagnosticView } from './EmulationDiagnosticView.vue'

/** @type {import('@box-bottom/web-components').TabDefinition} */
export const emulationTab = {
  id: 'emulation',
  label: 'Emulation',
  type: 'link',
  route: '/emulation',
  load: () => import('./EmulationDiagnosticView.vue'),
  visibility: () => !import.meta.env.PROD,
  enabled: () => !import.meta.env.PROD,
}
