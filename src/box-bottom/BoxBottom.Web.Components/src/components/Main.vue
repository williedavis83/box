<script setup>
defineProps({
  useRouterView: {
    type: Boolean,
    default: false,
  },
})
</script>

<template>
  <main class="box-main">
    <router-view v-if="useRouterView" />
    <slot v-else />
  </main>
</template>

<style scoped>
.box-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem 1.5rem;
  gap: 1rem;
}
</style>
