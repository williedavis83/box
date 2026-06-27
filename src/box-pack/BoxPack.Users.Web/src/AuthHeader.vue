<script setup>
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useUserMenuRegistry } from '@box-bottom/web-components'
import ZeroAuthLoginDialog from './ZeroAuthLoginDialog.vue'
import { useAuth } from './useAuth.js'

const router = useRouter()
const menuRegistry = useUserMenuRegistry()
const { session, provider, loading, isAuthenticated, loginWithEntra, loginWithZeroAuth, signOut } =
  useAuth()

const menuOpen = ref(false)
const zeroAuthOpen = ref(false)

const initials = computed(() => session.value?.initials ?? '?')

async function handleLoginClick() {
  if (provider.value === 'ZeroAuth') {
    zeroAuthOpen.value = true
    return
  }

  loginWithEntra()
}

async function handleZeroAuthSubmit(payload) {
  try {
    await loginWithZeroAuth(payload)
    zeroAuthOpen.value = false
  } catch (err) {
    throw err
  }
}

async function handleMenuSelect(item) {
  menuOpen.value = false
  await item.onSelect({
    router,
    signOut,
  })
}
</script>

<template>
  <div class="auth-header">
    <button
      v-if="!loading && !isAuthenticated"
      type="button"
      class="auth-header__login"
      @click="handleLoginClick"
    >
      Sign in
    </button>

    <div v-else-if="isAuthenticated" class="auth-header__user">
      <button
        type="button"
        class="auth-header__avatar"
        :aria-expanded="menuOpen"
        @click="menuOpen = !menuOpen"
      >
        {{ initials }}
      </button>

      <div v-if="menuOpen" class="auth-header__menu">
        <button
          v-for="item in menuRegistry.regularItems.value"
          :key="item.id"
          type="button"
          class="auth-header__menu-item"
          @click="handleMenuSelect(item)"
        >
          {{ item.label }}
        </button>

        <div v-if="menuRegistry.bottomItems.value.length" class="auth-header__menu-divider" />

        <button
          v-for="item in menuRegistry.bottomItems.value"
          :key="item.id"
          type="button"
          class="auth-header__menu-item"
          @click="handleMenuSelect(item)"
        >
          {{ item.label }}
        </button>
      </div>
    </div>

    <ZeroAuthLoginDialog
      v-model:open="zeroAuthOpen"
      @submit="handleZeroAuthSubmit"
    />
  </div>
</template>

<style scoped>
.auth-header {
  position: relative;
  display: flex;
  align-items: center;
}

.auth-header__login,
.auth-header__avatar {
  border: 1px solid #d0d5dd;
  border-radius: 999px;
  background: #fff;
  cursor: pointer;
}

.auth-header__login {
  padding: 0.5rem 0.875rem;
  font-size: 0.875rem;
}

.auth-header__avatar {
  width: 2.25rem;
  height: 2.25rem;
  font-size: 0.875rem;
  font-weight: 600;
  color: #175cd3;
}

.auth-header__menu {
  position: absolute;
  top: calc(100% + 0.5rem);
  right: 0;
  min-width: 12rem;
  padding: 0.375rem;
  border: 1px solid #e4e7ec;
  border-radius: 0.75rem;
  background: #fff;
  box-shadow: 0 12px 24px rgba(16, 24, 40, 0.12);
  z-index: 20;
}

.auth-header__menu-item {
  display: block;
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 0;
  border-radius: 0.5rem;
  background: transparent;
  text-align: left;
  cursor: pointer;
}

.auth-header__menu-item:hover {
  background: #f2f4f7;
}

.auth-header__menu-divider {
  margin: 0.375rem 0;
  border-top: 1px solid #e4e7ec;
}
</style>
