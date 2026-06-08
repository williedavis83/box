import { brandingConfig } from './BrandingConfig.js'

/**
 * Sets the document title from {@link brandingConfig}.
 *
 * @param {Document} [documentRef]
 */
export function applyWebTitle(documentRef = document) {
  documentRef.title = brandingConfig.title
}
