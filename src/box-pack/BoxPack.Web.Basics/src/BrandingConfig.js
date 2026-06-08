/**
 * Pack-specific branding for header and document metadata.
 *
 * Set `type` to `'image'` and provide `imageUrl` to use a logo instead of text.
 *
 * @typedef {'text' | 'image'} BrandingType
 *
 * @typedef {Object} BrandingConfig
 * @property {BrandingType} type
 * @property {string} [text] Display text when `type` is `'text'`.
 * @property {string | null} [imageUrl] Image URL when `type` is `'image'`.
 * @property {string} [imageAlt] Alt text when `type` is `'image'`.
 * @property {string} title Document `<title>` text.
 */

/** @type {BrandingConfig} */
export const brandingConfig = {
  type: 'text',
  text: 'BoxTop.Web',
  imageUrl: null,
  imageAlt: 'BoxTop.Web',
  title: 'BoxTop.Web',
}
