import { describe, expect, it } from 'vitest'
import {
  filterEnabledTabs,
  filterVisibleTabs,
  isTabEnabled,
  isTabVisible,
} from './tabPredicates.js'

const ctx = { env: { PROD: false } }

const visibleTab = {
  id: 'a',
  label: 'A',
  type: 'link',
  route: '/a',
  load: async () => ({ default: {} }),
}

const hiddenTab = {
  ...visibleTab,
  id: 'b',
  visibility: () => false,
}

const disabledTab = {
  ...visibleTab,
  id: 'c',
  enabled: () => false,
}

describe('tabPredicates', () => {
  it('isTabVisible respects visibility predicate', () => {
    expect(isTabVisible(visibleTab, ctx)).toBe(true)
    expect(isTabVisible(hiddenTab, ctx)).toBe(false)
  })

  it('isTabEnabled requires visible and enabled', () => {
    expect(isTabEnabled(visibleTab, ctx)).toBe(true)
    expect(isTabEnabled(hiddenTab, ctx)).toBe(false)
    expect(isTabEnabled(disabledTab, ctx)).toBe(false)
  })

  it('filterVisibleTabs and filterEnabledTabs exclude hidden/disabled tabs', () => {
    const tabs = [visibleTab, hiddenTab, disabledTab]

    expect(filterVisibleTabs(tabs, ctx).map((tab) => tab.id)).toEqual(['a', 'c'])
    expect(filterEnabledTabs(tabs, ctx).map((tab) => tab.id)).toEqual(['a'])
  })
})
