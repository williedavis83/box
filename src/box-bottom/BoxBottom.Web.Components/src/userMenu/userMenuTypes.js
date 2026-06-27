/**
 * @typedef {Object} UserMenuContext
 * @property {import('vue-router').Router} [router]
 * @property {() => Promise<void>} [signOut]
 *
 * @typedef {Object} UserMenuItem
 * @property {string} id
 * @property {string} label
 * @property {number} [order]
 * @property {boolean} [pinnedBottom]
 * @property {(ctx: UserMenuContext) => void | Promise<void>} onSelect
 */

export {}
