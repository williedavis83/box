import faviconUrl from '../assets/favicon.svg?url'

/**
 * Applies the pack favicon to the document.
 *
 * @param {Document} [documentRef]
 * @param {string} [href]
 */
export function applyFavicon(documentRef = document, href = faviconUrl) {
  let link = documentRef.querySelector("link[rel='icon']")

  if (!link) {
    link = documentRef.createElement('link')
    link.rel = 'icon'
    documentRef.head.appendChild(link)
  }

  link.type = 'image/svg+xml'
  link.href = href
}
