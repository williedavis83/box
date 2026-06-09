import { describe, expect, it } from 'vitest'
import { createComponentHost, requiresApiPredicate } from './componentHost.js'

describe('createComponentHost', () => {
  const catalog = [
    { logicalName: 'primary-api', url: 'http://box-primary-api' },
    { logicalName: 'meta', url: 'http://box-meta' },
  ]

  it('requiresApi returns true when all logical names are in the catalog', () => {
    const host = createComponentHost(catalog)

    expect(host.requiresApi('primary-api')).toBe(true)
    expect(host.requiresApi('primary-api', 'meta')).toBe(true)
    expect(host.requiresApi('Primary-API')).toBe(true)
  })

  it('requiresApi returns false when any logical name is missing', () => {
    const host = createComponentHost(catalog)

    expect(host.requiresApi('secondary-api')).toBe(false)
    expect(host.requiresApi('primary-api', 'secondary-api')).toBe(false)
  })

  it('requiresApiPredicate uses componentHost from tab context', () => {
    const host = createComponentHost(catalog)
    const predicate = requiresApiPredicate('primary-api')

    expect(predicate({ componentHost: host })).toBe(true)
    expect(predicate({ componentHost: createComponentHost([]) })).toBe(false)
    expect(predicate({})).toBe(false)
  })
})
