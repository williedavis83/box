<script setup>
import { onMounted, ref } from 'vue'

const props = defineProps({
  stackName: {
    type: String,
    default: '',
  },
})

const message = ref('')
const error = ref('')
const resolvedStackName = ref(props.stackName || globalThis.__BOX_STACK_NAME__ || '')

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
  <section class="hello-view">
    <p data-stack-name>{{ resolvedStackName }}</p>
    <p v-if="message" class="greeting">{{ message }}</p>
    <p v-else-if="error" class="error">{{ error }}</p>
    <p v-else>Loading...</p>
  </section>
</template>

<style scoped>
.hello-view {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.greeting {
  font-size: 1.5rem;
  font-weight: 600;
}

.error {
  color: #b42318;
}
</style>
