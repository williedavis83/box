import { describe, expect, it } from 'vitest'
import { splitTabsForOverflow } from './tabOverflow.js'

describe('splitTabsForOverflow', () => {
  it('keeps all tabs visible when they fit', () => {
    const result = splitTabsForOverflow(
      ['a', 'b', 'c'],
      { a: 40, b: 40, c: 40 },
      200,
      72,
      4,
    )

    expect(result.visibleIds).toEqual(['a', 'b', 'c'])
    expect(result.overflowIds).toEqual([])
  })

  it('moves rightmost tabs into overflow when space is limited', () => {
    const result = splitTabsForOverflow(
      ['a', 'b', 'c', 'd'],
      { a: 60, b: 60, c: 60, d: 60 },
      180,
      72,
      4,
    )

    expect(result.visibleIds.length).toBeLessThan(4)
    expect(result.overflowIds.length).toBeGreaterThan(0)
    expect([...result.visibleIds, ...result.overflowIds]).toEqual(['a', 'b', 'c', 'd'])
  })

  it('puts all tabs in overflow when none fit with More reserved', () => {
    const result = splitTabsForOverflow(
      ['a', 'b'],
      { a: 100, b: 100 },
      120,
      72,
      4,
    )

    expect(result.visibleIds).toEqual([])
    expect(result.overflowIds).toEqual(['a', 'b'])
  })

  it('accounts for flex gap between tabs', () => {
    const tabIds = ['a', 'b', 'c']
    const tabWidths = { a: 40, b: 40, c: 40 }

    const withoutGap = splitTabsForOverflow(tabIds, tabWidths, 125, 72, 0)
    const withGap = splitTabsForOverflow(tabIds, tabWidths, 125, 72, 4)

    expect(withoutGap.visibleIds).toEqual(['a', 'b', 'c'])
    expect(withGap.visibleIds).toEqual(['a'])
    expect(withGap.overflowIds).toEqual(['b', 'c'])
  })

  it('does not reserve More width when all remaining tabs still fit', () => {
    const result = splitTabsForOverflow(
      ['a', 'b', 'c'],
      { a: 40, b: 40, c: 40 },
      128,
      72,
      4,
    )

    expect(result.visibleIds).toEqual(['a', 'b', 'c'])
    expect(result.overflowIds).toEqual([])
  })
})
