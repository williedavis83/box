<script setup>
import { inject, ref } from 'vue'
import { REGISTER_DYNAMIC_TAB_KEY } from '@box-bottom/web-components'

const registerDynamicTab = inject(REGISTER_DYNAMIC_TAB_KEY, null)
const extraCount = ref(0)

function addExtraTab() {
  extraCount.value += 1
  const index = extraCount.value

  const tab = {
    id: `extra-${index}`,
    label: `Extra ${index}`,
    type: 'link',
    route: `/test/extra-${index}`,
    load: () => import('./ExtraTabView.vue'),
    visibility: () => true,
    enabled: () => true,
    meta: { extraLabel: `Extra tab ${index}` },
  }

  registerDynamicTab?.(tab)
}
</script>

<template>
  <section class="test-tab-panel">
    <h2>Test tab</h2>
    <p>Use this tab to add more tabs and exercise overflow behavior.</p>
    <button type="button" data-testid="add-tab-button" @click="addExtraTab">
      Add tab
    </button>
    <p>Added: {{ extraCount }}</p>
  </section>
</template>

<style scoped>
.test-tab-panel {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}
</style>
