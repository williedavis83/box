<script setup>
import { ref } from 'vue'

const open = defineModel('open', { type: Boolean, default: false })

const emit = defineEmits(['submit'])

const provider = ref('ZeroAuth')
const externalId = ref('dev-user')
const displayName = ref('Dev User')
const email = ref('dev@example.com')
const error = ref(null)
const submitting = ref(false)

async function handleSubmit() {
  error.value = null
  submitting.value = true

  try {
    emit('submit', {
      provider: provider.value.trim(),
      externalId: externalId.value.trim(),
      displayName: displayName.value.trim(),
      email: email.value.trim(),
    })
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Login failed.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div v-if="open" class="zero-auth-dialog">
    <div class="zero-auth-dialog__backdrop" @click="open = false" />
    <form class="zero-auth-dialog__panel" @submit.prevent="handleSubmit">
      <h2 class="zero-auth-dialog__title">ZeroAuth Login</h2>
      <p class="zero-auth-dialog__hint">Development login using a JSON user description.</p>

      <label class="zero-auth-dialog__field">
        <span>Provider</span>
        <input v-model="provider" required />
      </label>

      <label class="zero-auth-dialog__field">
        <span>External ID</span>
        <input v-model="externalId" required />
      </label>

      <label class="zero-auth-dialog__field">
        <span>Display name</span>
        <input v-model="displayName" required />
      </label>

      <label class="zero-auth-dialog__field">
        <span>Email</span>
        <input v-model="email" type="email" required />
      </label>

      <p v-if="error" class="zero-auth-dialog__error">{{ error }}</p>

      <div class="zero-auth-dialog__actions">
        <button type="button" class="zero-auth-dialog__button" @click="open = false">
          Cancel
        </button>
        <button type="submit" class="zero-auth-dialog__button zero-auth-dialog__button--primary" :disabled="submitting">
          Sign in
        </button>
      </div>
    </form>
  </div>
</template>

<style scoped>
.zero-auth-dialog {
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
}

.zero-auth-dialog__backdrop {
  position: absolute;
  inset: 0;
  background: rgba(16, 24, 40, 0.45);
}

.zero-auth-dialog__panel {
  position: relative;
  width: min(420px, calc(100vw - 2rem));
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 1.25rem;
  border-radius: 0.75rem;
  background: #fff;
  box-shadow: 0 20px 40px rgba(16, 24, 40, 0.18);
}

.zero-auth-dialog__title {
  margin: 0;
  font-size: 1.125rem;
}

.zero-auth-dialog__hint {
  margin: 0;
  color: #667085;
  font-size: 0.875rem;
}

.zero-auth-dialog__field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.875rem;
}

.zero-auth-dialog__field input {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d0d5dd;
  border-radius: 0.5rem;
}

.zero-auth-dialog__error {
  margin: 0;
  color: #b42318;
  font-size: 0.875rem;
}

.zero-auth-dialog__actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
}

.zero-auth-dialog__button {
  padding: 0.5rem 0.875rem;
  border: 1px solid #d0d5dd;
  border-radius: 0.5rem;
  background: #fff;
  cursor: pointer;
}

.zero-auth-dialog__button--primary {
  border-color: #175cd3;
  background: #175cd3;
  color: #fff;
}
</style>
