/**
 * @typedef {{ account: string, bar: string, baz: string }} BlabberResult
 * @typedef {{ accounts: Record<string, BlabberResult> }} BlabberDictResult
 */

/**
 * @param {string} path
 * @returns {Promise<unknown>}
 */
async function fetchJson(path) {
  const response = await fetch(path)
  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

  return response.json()
}

/** @returns {Promise<BlabberResult>} */
export function fetchFoo() {
  return fetchJson('/api/Blabber/Foo')
}

/** @returns {Promise<BlabberResult>} */
export function fetchFee() {
  return fetchJson('/api/Blabber/Fee')
}

/** @returns {Promise<BlabberResult[]>} */
export function fetchListA() {
  return fetchJson('/api/Blabber/ListA')
}

/** @returns {Promise<BlabberResult[]>} */
export function fetchListB() {
  return fetchJson('/api/Blabber/ListB')
}

/** @returns {Promise<BlabberDictResult>} */
export function fetchDictA() {
  return fetchJson('/api/Blabber/DictA')
}

/** @returns {Promise<BlabberDictResult>} */
export function fetchDictB() {
  return fetchJson('/api/Blabber/DictB')
}

/** @returns {Promise<{ foo: BlabberResult, fee: BlabberResult, listA: BlabberResult[], listB: BlabberResult[], dictA: BlabberDictResult, dictB: BlabberDictResult }>} */
export async function fetchAllBlabberData() {
  const [foo, fee, listA, listB, dictA, dictB] = await Promise.all([
    fetchFoo(),
    fetchFee(),
    fetchListA(),
    fetchListB(),
    fetchDictA(),
    fetchDictB(),
  ])

  return { foo, fee, listA, listB, dictA, dictB }
}
