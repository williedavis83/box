<script setup>
import { useRoute } from 'vue-router'
import { computed } from 'vue'

const route = useRoute()
const label = computed(() => route.meta.extraLabel ?? route.params.id ?? 'Extra tab')
</script>

<template>
  <section class="extra-tab-view">
    <h2>{{ label }}</h2>
    <p data-testid="extra-tab-content">Dynamic tab content</p>
  </section>
</template>

<style scoped>
.extra-tab-view {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
}
</style>
