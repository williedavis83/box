import { describe, expect, it } from 'vitest'
import { createUserMenuRegistry } from './useUserMenuRegistry.js'

describe('createUserMenuRegistry', () => {
  it('sorts regular items before pinned bottom items by order', () => {
    const registry = createUserMenuRegistry([
      { id: 'sign-out', label: 'Sign out', order: 100, pinnedBottom: true, onSelect: () => {} },
      { id: 'profile', label: 'User Profile', order: 10, onSelect: () => {} },
    ])

    expect(registry.regularItems.value.map((item) => item.id)).toEqual(['profile'])
    expect(registry.bottomItems.value.map((item) => item.id)).toEqual(['sign-out'])
  })
})
