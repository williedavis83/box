/**
 * @typedef {{ logicalName: string, url: string }} MetaApiEntry
 */

/**
 * @typedef {Object} ComponentHost
 * @property {(...logicalNames: string[]) => boolean} requiresApi
 * @property {() => MetaApiEntry[]} getCatalog
 */

/**
 * @param {MetaApiEntry[]} [catalog]
 * @returns {ComponentHost}
 */
export function createComponentHost(catalog = []) {
  const normalized = new Set(
    catalog.map((entry) => entry.logicalName.trim().toLowerCase()),
  )

  return {
    requiresApi(...logicalNames) {
      return logicalNames.every((name) =>
        normalized.has(name.trim().toLowerCase()),
      )
    },
    getCatalog() {
      return [...catalog]
    },
  }
}

/**
 * @param {string} [baseUrl]
 * @returns {Promise<MetaApiEntry[]>}
 */
export async function fetchMetaApiCatalog(baseUrl = '/api/meta') {
  const response = await fetch(`${baseUrl.replace(/\/$/, '')}/apis`)

  if (!response.ok) {
    throw new Error(`Failed to load meta API catalog: ${response.status}`)
  }

  return /** @type {MetaApiEntry[]} */ (await response.json())
}

/**
 * @param {string} [baseUrl]
 * @returns {Promise<ComponentHost>}
 */
export async function loadComponentHost(baseUrl = '/api/meta') {
  const catalog = await fetchMetaApiCatalog(baseUrl)
  return createComponentHost(catalog)
}

/**
 * @param {...string} logicalNames
 * @returns {import('../tabs/tabTypes.js').TabPredicate}
 */
export function requiresApiPredicate(...logicalNames) {
  return (ctx) => ctx.componentHost?.requiresApi(...logicalNames) ?? false
}
