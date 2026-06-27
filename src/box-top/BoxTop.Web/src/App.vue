<script setup>
import { onMounted, provide, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import {
  COMPONENT_HOST_KEY,
  createComponentHost,
  createTabRegistry,
  createUserMenuRegistry,
  Footer,
  Header,
  loadComponentHost,
  Main,
  REGISTER_DYNAMIC_TAB_KEY,
  TAB_REGISTRY_KEY,
  USER_MENU_REGISTRY_KEY,
} from '@box-bottom/web-components'
import { brandingConfig } from '@box-pack/web-basics'
import { addRouteForTab, initialTabs } from '@box-pack/web-navigation'
import { AuthHeader, createDefaultUserMenuItems, userProfileTab } from '@box-pack/users-web'

const route = useRoute()
const componentHost = ref(createComponentHost([]))
const registry = createTabRegistry(initialTabs, { env: import.meta.env })
const userMenuRegistry = createUserMenuRegistry(createDefaultUserMenuItems())

function syncRegistryContext() {
  registry.setContext({
    env: import.meta.env,
    componentHost: componentHost.value,
    route: route.path,
  })
}

function syncUserProfileTab(path) {
  if (path === '/user-profile') {
    registry.addTab(userProfileTab)
  } else {
    registry.removeTab(userProfileTab.id)
  }
}

watch(
  [() => route.path, componentHost],
  ([path]) => {
    syncRegistryContext()
    syncUserProfileTab(path)
  },
  { immediate: true },
)

onMounted(async () => {
  try {
    componentHost.value = await loadComponentHost()
  } catch {
    componentHost.value = createComponentHost([])
  }
})

provide(TAB_REGISTRY_KEY, registry)
provide(USER_MENU_REGISTRY_KEY, userMenuRegistry)
provide(COMPONENT_HOST_KEY, componentHost)
provide(REGISTER_DYNAMIC_TAB_KEY, (tab) => {
  registry.addTab(tab)
  addRouteForTab(tab)
})
</script>

<template>
  <div class="layout">
    <Header :branding="brandingConfig">
      <template #auth>
        <AuthHeader />
      </template>
    </Header>
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
