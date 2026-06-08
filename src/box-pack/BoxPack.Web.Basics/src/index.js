export { brandingConfig } from './BrandingConfig.js'
export { applyWebTitle } from './WebTitle.js'
export { applyFavicon } from './Favicon.js'

import { applyFavicon } from './Favicon.js'
import { applyWebTitle } from './WebTitle.js'

export function applyWebBasics(documentRef = document) {
  applyWebTitle(documentRef)
  applyFavicon(documentRef)
}
