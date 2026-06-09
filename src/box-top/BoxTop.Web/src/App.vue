<script setup>
import { onMounted, provide, ref, watch } from 'vue'
import {
  COMPONENT_HOST_KEY,
  createComponentHost,
  createTabRegistry,
  Footer,
  Header,
  loadComponentHost,
  Main,
  REGISTER_DYNAMIC_TAB_KEY,
  TAB_REGISTRY_KEY,
} from '@box-bottom/web-components'
import { brandingConfig } from '@box-pack/web-basics'
import { initialTabs } from '@box-pack/web-navigation'
import { addRouteForTab } from './router.js'

const componentHost = ref(createComponentHost([]))
const registry = createTabRegistry(initialTabs, { env: import.meta.env })

function syncRegistryContext() {
  registry.setContext({
    env: import.meta.env,
    componentHost: componentHost.value,
  })
}

watch(componentHost, syncRegistryContext, { immediate: true })

onMounted(async () => {
  try {
    componentHost.value = await loadComponentHost()
  } catch {
    componentHost.value = createComponentHost([])
  }
})

provide(TAB_REGISTRY_KEY, registry)
provide(COMPONENT_HOST_KEY, componentHost)
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
