<script setup>
import { computed, defineAsyncComponent, ref, shallowRef, watch } from 'vue'

const props = defineProps({
  tab: {
    type: Object,
    required: true,
  },
  disabled: {
    type: Boolean,
    default: false,
  },
})

const isOpen = ref(false)
const panelComponent = shallowRef(null)

const asyncPanel = computed(() => {
  if (!panelComponent.value) {
    return null
  }

  return defineAsyncComponent(() => Promise.resolve({ default: panelComponent.value }))
})

watch(isOpen, async (open) => {
  if (!open || panelComponent.value || !props.tab.load) {
    return
  }

  const module = await props.tab.load()
  panelComponent.value = module.default
})

function toggle() {
  if (props.disabled) {
    return
  }

  isOpen.value = !isOpen.value
}

function close() {
  isOpen.value = false
}
</script>

<template>
  <div class="box-menu-tab">
    <button
      type="button"
      class="box-menu-tab__button"
      :class="{ 'box-menu-tab__button--open': isOpen }"
      :data-tab-id="tab.id"
      :disabled="disabled"
      @click="toggle"
    >
      {{ tab.label }}
    </button>
    <div v-if="isOpen" class="box-menu-tab__panel">
      <component :is="asyncPanel" v-if="asyncPanel" @close="close" />
    </div>
  </div>
</template>

<style scoped>
.box-menu-tab {
  position: relative;
}

.box-menu-tab__button {
  border: none;
  background: transparent;
  padding: 0.5rem 0.75rem;
  font: inherit;
  cursor: pointer;
  white-space: nowrap;
  border-bottom: 2px solid transparent;
  flex-shrink: 0;
}

.box-menu-tab__button--open {
  border-bottom-color: #863bff;
  font-weight: 600;
}

.box-menu-tab__button:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.box-menu-tab__panel {
  position: absolute;
  top: calc(100% + 0.25rem);
  left: 0;
  z-index: 20;
  min-width: 12rem;
  padding: 0.75rem;
  border: 1px solid #e4e7ec;
  border-radius: 0.5rem;
  background: #fff;
  box-shadow: 0 8px 24px rgba(16, 24, 40, 0.12);
}
</style>
