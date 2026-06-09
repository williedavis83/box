<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useComponentHost } from '@box-bottom/web-components'

const props = defineProps({
  stackName: {
    type: String,
    default: '',
  },
})

const componentHost = useComponentHost()
const canRender = computed(() => componentHost.value.requiresApi('primary-api'))

const message = ref('')
const error = ref('')
const resolvedStackName = ref(props.stackName || globalThis.__BOX_STACK_NAME__ || '')

async function loadGreeting() {
  message.value = ''
  error.value = ''

  try {
    const response = await fetch('/api/Hello')
    if (!response.ok) {
      throw new Error(`Request failed with status ${response.status}`)
    }

    message.value = await response.text()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load greeting'
  }
}

onMounted(() => {
  if (canRender.value) {
    loadGreeting()
  }
})

watch(canRender, (isAllowed) => {
  if (isAllowed) {
    loadGreeting()
  }
})
</script>

<template>
  <section v-if="canRender" class="hello-view">
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
