import { computed, onMounted, ref } from 'vue'
import {
  fetchAuthConfig,
  fetchAuthSession,
  logout as logoutRequest,
  startEntraLogin,
  zeroAuthLogin,
} from './authApi.js'

export function useAuth() {
  const session = ref(null)
  const provider = ref(null)
  const loading = ref(true)
  const error = ref(null)

  const isAuthenticated = computed(() => session.value !== null)

  async function refresh() {
    loading.value = true
    error.value = null

    try {
      const [config, currentSession] = await Promise.all([
        fetchAuthConfig(),
        fetchAuthSession(),
      ])

      provider.value = config?.provider ?? null
      session.value = currentSession
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load auth state.'
      session.value = null
    } finally {
      loading.value = false
    }
  }

  async function loginWithZeroAuth(payload) {
    session.value = await zeroAuthLogin(payload)
  }

  function loginWithEntra() {
    startEntraLogin(window.location.pathname)
  }

  async function signOut() {
    await logoutRequest()
    session.value = null
  }

  onMounted(refresh)

  return {
    session,
    provider,
    loading,
    error,
    isAuthenticated,
    refresh,
    loginWithZeroAuth,
    loginWithEntra,
    signOut,
  }
}
