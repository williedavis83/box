/** @type {import('@box-bottom/web-components').UserMenuItem[]} */
export function createDefaultUserMenuItems() {
  return [
    {
      id: 'user-profile',
      label: 'User Profile',
      order: 10,
      onSelect: ({ router }) => {
        router?.push('/user-profile')
      },
    },
    {
      id: 'sign-out',
      label: 'Sign out',
      order: 100,
      pinnedBottom: true,
      onSelect: async ({ signOut }) => {
        await signOut?.()
      },
    },
  ]
}
