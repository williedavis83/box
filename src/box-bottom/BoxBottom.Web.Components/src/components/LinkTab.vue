<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

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

const router = useRouter()
const route = useRoute()

const isActive = computed(() => route.path === props.tab.route)

function activate() {
  if (props.disabled) {
    return
  }

  router.push(props.tab.route)
}
</script>

<template>
  <button
    type="button"
    class="box-link-tab"
    :class="{ 'box-link-tab--active': isActive }"
    :data-tab-id="tab.id"
    :disabled="disabled"
    @click="activate"
  >
    {{ tab.label }}
  </button>
</template>

<style scoped>
.box-link-tab {
  border: none;
  background: transparent;
  padding: 0.5rem 0.75rem;
  font: inherit;
  cursor: pointer;
  white-space: nowrap;
  border-bottom: 2px solid transparent;
  flex-shrink: 0;
}

.box-link-tab--active {
  border-bottom-color: #863bff;
  font-weight: 600;
}

.box-link-tab:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}
</style>
