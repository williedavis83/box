<script setup>
import { onMounted, ref } from 'vue'

const message = ref('')
const error = ref('')

onMounted(async () => {
  try {
    const response = await fetch('/api/Hello')
    if (!response.ok) {
      throw new Error(`Request failed with status ${response.status}`)
    }

    message.value = await response.text()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load greeting'
  }
})
</script>

<template>
  <main class="page">
    <h1>BoxTop.Web</h1>
    <p v-if="message" class="greeting">{{ message }}</p>
    <p v-else-if="error" class="error">{{ error }}</p>
    <p v-else>Loading...</p>
  </main>
</template>
