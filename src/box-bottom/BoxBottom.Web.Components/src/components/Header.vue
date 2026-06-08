<script setup>
import BrandingComponent from './BrandingComponent.vue'
import HeaderPlaceholder from './HeaderPlaceholder.vue'
import TabMenu from './TabMenu.vue'

defineProps({
  branding: {
    type: Object,
    required: true,
  },
})
</script>

<template>
  <header class="box-header">
    <BrandingComponent :branding="branding" />
    <TabMenu class="box-header__tabs" />
    <HeaderPlaceholder class="box-header__placeholder" />
  </header>
</template>

<style scoped>
.box-header {
  display: flex;
  align-items: stretch;
  gap: 1rem;
  padding: 0.75rem 1.5rem;
  border-bottom: 1px solid #e4e7ec;
}

.box-header__tabs {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: stretch;
}

.box-header__placeholder {
  align-self: center;
}
</style>
