<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useComponentHost } from '@box-bottom/web-components'
import { fetchAllBlabberData } from './blabberApi.js'

const componentHost = useComponentHost()
const canRender = computed(() => componentHost.value.requiresApi('primary-api'))

const data = ref(null)
const error = ref('')
const loading = ref(false)

async function loadBlabberData() {
  loading.value = true
  error.value = ''
  data.value = null

  try {
    data.value = await fetchAllBlabberData()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load blabber data'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  if (canRender.value) {
    loadBlabberData()
  }
})

watch(canRender, (isAllowed) => {
  if (isAllowed) {
    loadBlabberData()
  }
})

function dictEntries(dictResult) {
  return Object.entries(dictResult?.accounts ?? {})
}
</script>

<template>
  <section v-if="canRender" class="blabber-view">
    <header class="blabber-view__header">
      <h2>Blabber API</h2>
      <p>Results from the primary API Blabber endpoints.</p>
    </header>

    <p v-if="loading" data-testid="blabber-loading">Loading...</p>
    <p v-else-if="error" class="blabber-view__error" data-testid="blabber-error">{{ error }}</p>

    <div v-else-if="data" class="blabber-view__sections">
      <section class="blabber-section">
        <h3>Foo</h3>
        <dl class="blabber-result">
          <div><dt>Account</dt><dd>{{ data.foo.account }}</dd></div>
          <div><dt>Bar</dt><dd>{{ data.foo.bar }}</dd></div>
          <div><dt>Baz</dt><dd>{{ data.foo.baz }}</dd></div>
        </dl>
      </section>

      <section class="blabber-section">
        <h3>Fee</h3>
        <dl class="blabber-result">
          <div><dt>Account</dt><dd>{{ data.fee.account }}</dd></div>
          <div><dt>Bar</dt><dd>{{ data.fee.bar }}</dd></div>
          <div><dt>Baz</dt><dd>{{ data.fee.baz }}</dd></div>
        </dl>
      </section>

      <section class="blabber-section">
        <h3>ListA</h3>
        <ul class="blabber-list">
          <li v-for="item in data.listA" :key="`list-a-${item.account}`">
            {{ item.account }}: bar={{ item.bar }}, baz={{ item.baz }}
          </li>
        </ul>
      </section>

      <section class="blabber-section">
        <h3>ListB</h3>
        <ul class="blabber-list">
          <li v-for="item in data.listB" :key="`list-b-${item.account}`">
            {{ item.account }}: bar={{ item.bar }}, baz={{ item.baz }}
          </li>
        </ul>
      </section>

      <section class="blabber-section">
        <h3>DictA</h3>
        <ul class="blabber-list">
          <li v-for="[account, item] in dictEntries(data.dictA)" :key="`dict-a-${account}`">
            {{ account }}: bar={{ item.bar }}, baz={{ item.baz }}
          </li>
        </ul>
      </section>

      <section class="blabber-section">
        <h3>DictB</h3>
        <ul class="blabber-list">
          <li v-for="[account, item] in dictEntries(data.dictB)" :key="`dict-b-${account}`">
            {{ account }}: bar={{ item.bar }}, baz={{ item.baz }}
          </li>
        </ul>
      </section>
    </div>
  </section>
</template>

<style scoped>
.blabber-view {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 1rem;
  max-width: 48rem;
  margin: 0 auto;
}

.blabber-view__header h2 {
  margin: 0 0 0.25rem;
}

.blabber-view__header p {
  margin: 0;
  color: #667085;
}

.blabber-view__sections {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.blabber-section {
  border: 1px solid #e4e7ec;
  border-radius: 0.5rem;
  padding: 1rem;
}

.blabber-section h3 {
  margin: 0 0 0.75rem;
}

.blabber-result {
  display: grid;
  gap: 0.5rem;
  margin: 0;
}

.blabber-result div {
  display: grid;
  grid-template-columns: 5rem 1fr;
  gap: 0.5rem;
}

.blabber-result dt {
  font-weight: 600;
}

.blabber-result dd {
  margin: 0;
}

.blabber-list {
  margin: 0;
  padding-left: 1.25rem;
}

.blabber-view__error {
  color: #b42318;
}
</style>
