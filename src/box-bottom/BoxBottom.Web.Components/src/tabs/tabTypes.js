/**
 * @typedef {'link' | 'menu'} TabType
 *
 * @typedef {Object} TabContext
 * @property {ImportMetaEnv} [env]
 * @property {import('../host/componentHost.js').ComponentHost} [componentHost]
 * @property {string} [route]
 *
 * @typedef {(ctx: TabContext) => boolean} TabPredicate
 *
 * @typedef {Object} TabDefinitionBase
 * @property {string} id
 * @property {string} label
 * @property {TabType} type
 * @property {TabPredicate} [visibility]
 * @property {TabPredicate} [enabled]
 * @property {() => Promise<{ default: import('vue').Component }>} load
 *
 * @typedef {TabDefinitionBase & { type: 'link', route: string }} LinkTabDefinition
 *
 * @typedef {TabDefinitionBase & { type: 'menu' }} MenuTabDefinition
 *
 * @typedef {LinkTabDefinition | MenuTabDefinition} TabDefinition
 */

export {}
