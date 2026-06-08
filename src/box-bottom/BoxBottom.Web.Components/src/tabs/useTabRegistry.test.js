import { describe, expect, it } from 'vitest'
import { createTabRegistry } from './useTabRegistry.js'

const baseTab = {
  id: 'hello',
  label: 'Hello',
  type: 'link',
  route: '/hello',
  load: async () => ({ default: {} }),
  visibility: () => true,
  enabled: () => true,
}

describe('createTabRegistry', () => {
  it('adds tabs dynamically without duplicates', () => {
    const registry = createTabRegistry([baseTab], { env: { PROD: false } })

    registry.addTab({
      ...baseTab,
      id: 'test',
      label: 'Test',
      route: '/test',
      visibility: () => !import.meta.env.PROD,
    })

    registry.addTab({
      ...baseTab,
      id: 'test',
      label: 'Test duplicate',
      route: '/test-dup',
    })

    expect(registry.tabs.value.map((tab) => tab.id)).toEqual(['hello', 'test'])
  })

  it('re-evaluates visibility when context changes', () => {
    const registry = createTabRegistry([
      {
        ...baseTab,
        id: 'dev-only',
        visibility: (ctx) => !ctx.env?.PROD,
      },
    ], { env: { PROD: true } })

    expect(registry.visibleTabs.value).toHaveLength(0)

    registry.setContext({ env: { PROD: false } })
    expect(registry.visibleTabs.value).toHaveLength(1)
  })

  it('removeTab drops tabs by id', () => {
    const registry = createTabRegistry([baseTab], { env: {} })
    registry.removeTab('hello')
    expect(registry.tabs.value).toHaveLength(0)
  })
})
