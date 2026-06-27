import { computed, inject, ref } from 'vue'

export const USER_MENU_REGISTRY_KEY = Symbol('userMenuRegistry')

/**
 * @param {import('./userMenuTypes.js').UserMenuItem[]} initialItems
 */
export function createUserMenuRegistry(initialItems = []) {
  const items = ref([...initialItems])

  const sortedItems = computed(() =>
    [...items.value].sort((left, right) => {
      const leftOrder = left.order ?? 0
      const rightOrder = right.order ?? 0
      if (leftOrder !== rightOrder) {
        return leftOrder - rightOrder
      }

      return left.label.localeCompare(right.label)
    }),
  )

  const regularItems = computed(() =>
    sortedItems.value.filter((item) => !item.pinnedBottom),
  )

  const bottomItems = computed(() =>
    sortedItems.value.filter((item) => item.pinnedBottom),
  )

  /**
   * @param {import('./userMenuTypes.js').UserMenuItem} item
   */
  function addItem(item) {
    if (items.value.some((existing) => existing.id === item.id)) {
      return
    }

    items.value = [...items.value, item]
  }

  return {
    items,
    sortedItems,
    regularItems,
    bottomItems,
    addItem,
  }
}

export function useUserMenuRegistry() {
  const registry = inject(USER_MENU_REGISTRY_KEY)

  if (!registry) {
    throw new Error('useUserMenuRegistry must be used within a user menu registry provider')
  }

  return registry
}
