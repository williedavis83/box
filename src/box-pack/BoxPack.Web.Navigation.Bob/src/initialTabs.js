import { emulationTab } from '@box-bottom/web-emulation'
import { azureTableTab } from '@foo/azure-table-web'
import { blabberTab } from '@foo/blabber-web'
import { helloTab } from '@foo/web-hello'

/** Bob product tabs — same shape as box navigation, without the Test overflow demo tab. */
export const initialTabs = [helloTab, blabberTab, azureTableTab, emulationTab]

export { azureTableTab, blabberTab, emulationTab, helloTab }
