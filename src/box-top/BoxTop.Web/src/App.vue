<script setup>
import { provide } from 'vue'
import {
  createTabRegistry,
  Footer,
  Header,
  Main,
  REGISTER_DYNAMIC_TAB_KEY,
  TAB_REGISTRY_KEY,
} from '@box-bottom/web-components'
import { brandingConfig } from '@box-pack/web-basics'
import { initialTabs } from '@box-pack/web-navigation'
import { addRouteForTab } from './router.js'

const registry = createTabRegistry(initialTabs, { env: import.meta.env })

provide(TAB_REGISTRY_KEY, registry)
provide(REGISTER_DYNAMIC_TAB_KEY, (tab) => {
  registry.addTab(tab)
  addRouteForTab(tab)
})
</script>

<template>
  <div class="layout">
    <Header :branding="brandingConfig" />
    <Main use-router-view />
    <Footer />
  </div>
</template>

<style scoped>
.layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}
</style>
