<script setup>
import { onMounted, ref } from 'vue'
import { fetchMyProfile, updateMyProfile } from './authApi.js'

const profile = ref(null)
const displayName = ref('')
const email = ref('')
const loading = ref(true)
const saving = ref(false)
const error = ref(null)
const message = ref(null)

async function loadProfile() {
  loading.value = true
  error.value = null

  try {
    const loaded = await fetchMyProfile()
    profile.value = loaded
    displayName.value = loaded?.displayName ?? ''
    email.value = loaded?.email ?? ''
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load profile.'
  } finally {
    loading.value = false
  }
}

async function handleSave() {
  saving.value = true
  error.value = null
  message.value = null

  try {
    profile.value = await updateMyProfile({
      displayName: displayName.value.trim(),
      email: email.value.trim(),
    })
    message.value = 'Profile saved.'
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to save profile.'
  } finally {
    saving.value = false
  }
}

onMounted(loadProfile)
</script>

<template>
  <section class="user-profile-view">
    <h1>User Profile</h1>

    <p v-if="loading">Loading profile...</p>
    <p v-else-if="error" class="user-profile-view__error">{{ error }}</p>

    <form v-else class="user-profile-view__form" @submit.prevent="handleSave">
      <label class="user-profile-view__field">
        <span>Display name</span>
        <input v-model="displayName" required />
      </label>

      <label class="user-profile-view__field">
        <span>Email</span>
        <input v-model="email" type="email" required />
      </label>

      <p v-if="message" class="user-profile-view__message">{{ message }}</p>

      <button type="submit" class="user-profile-view__save" :disabled="saving">
        Save profile
      </button>
    </form>
  </section>
</template>

<style scoped>
.user-profile-view {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  max-width: 32rem;
}

.user-profile-view__form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.user-profile-view__field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.user-profile-view__field input {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d0d5dd;
  border-radius: 0.5rem;
}

.user-profile-view__save {
  align-self: flex-start;
  padding: 0.5rem 0.875rem;
  border: 0;
  border-radius: 0.5rem;
  background: #175cd3;
  color: #fff;
  cursor: pointer;
}

.user-profile-view__error {
  color: #b42318;
}

.user-profile-view__message {
  color: #027a48;
}
</style>
