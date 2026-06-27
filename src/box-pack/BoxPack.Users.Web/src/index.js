export { default as AuthHeader } from './AuthHeader.vue'
export { default as UserProfileView } from './UserProfileView.vue'
export { default as ZeroAuthLoginDialog } from './ZeroAuthLoginDialog.vue'
export {
  fetchAuthConfig,
  fetchAuthSession,
  fetchMyProfile,
  logout,
  startEntraLogin,
  updateMyProfile,
  zeroAuthLogin,
} from './authApi.js'
export { useAuth } from './useAuth.js'
export { createDefaultUserMenuItems } from './userMenuItems.js'

import { requiresApiPredicate } from '@box-bottom/web-components'

/** @type {import('@box-bottom/web-components').TabDefinition} */
export const userProfileTab = {
  id: 'user-profile',
  label: 'User Profile',
  type: 'link',
  route: '/user-profile',
  load: () => import('./UserProfileView.vue'),
  visibility: requiresApiPredicate('users-api'),
  enabled: requiresApiPredicate('users-api'),
}
