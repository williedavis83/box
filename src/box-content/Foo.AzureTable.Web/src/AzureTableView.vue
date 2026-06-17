<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useComponentHost } from '@box-bottom/web-components'
import {
  ensureAzureTable,
  fetchAzureTableEntities,
  fetchAzureTables,
  fetchAzureTableStatus,
  upsertAzureTableEntity,
} from './azureTableApi.js'

const componentHost = useComponentHost()
const canRender = computed(() => componentHost.value.requiresApi('primary-api'))

const status = ref(null)
const tables = ref([])
const entities = ref([])
const tableName = ref('boxBasics')
const partitionKey = ref('demo')
const rowKey = ref('sample')
const propertyValue = ref('hello-from-web')
const error = ref('')
const loading = ref(false)
const actionMessage = ref('')

async function loadAzureTableData() {
  loading.value = true
  error.value = ''
  actionMessage.value = ''
  tables.value = []
  entities.value = []

  try {
    status.value = await fetchAzureTableStatus()

    if (!status.value.isConfigured) {
      return
    }

    tables.value = await fetchAzureTables()
    await refreshEntities()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load Azure Table data'
  } finally {
    loading.value = false
  }
}

async function refreshEntities() {
  if (!status.value?.isConfigured || !tableName.value) {
    entities.value = []
    return
  }

  entities.value = await fetchAzureTableEntities(tableName.value)
}

async function handleEnsureTable() {
  error.value = ''
  actionMessage.value = ''

  try {
    await ensureAzureTable(tableName.value)
    actionMessage.value = `Ensured table '${tableName.value}'.`
    tables.value = await fetchAzureTables()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to ensure table'
  }
}

async function handleUpsertEntity() {
  error.value = ''
  actionMessage.value = ''

  try {
    await upsertAzureTableEntity(tableName.value, {
      partitionKey: partitionKey.value,
      rowKey: rowKey.value,
      properties: {
        message: propertyValue.value,
      },
    })

    actionMessage.value = `Upserted entity ${partitionKey.value}/${rowKey.value}.`
    await refreshEntities()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to upsert entity'
  }
}

onMounted(() => {
  if (canRender.value) {
    loadAzureTableData()
  }
})

watch(canRender, (isAllowed) => {
  if (isAllowed) {
    loadAzureTableData()
  }
})

watch(tableName, async () => {
  if (status.value?.isConfigured) {
    try {
      await refreshEntities()
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load entities'
    }
  }
})
</script>

<template>
  <section v-if="canRender" class="azure-table-view">
    <header class="azure-table-view__header">
      <h2>Azure Table Basics</h2>
      <p>Exercise the primary API Azure Table endpoints.</p>
    </header>

    <p v-if="loading" data-testid="azure-table-loading">Loading...</p>
    <p v-else-if="error" class="azure-table-view__error" data-testid="azure-table-error">{{ error }}</p>

    <div v-else class="azure-table-view__content">
      <section class="azure-table-panel">
        <h3>Configuration</h3>
        <dl class="azure-table-status">
          <div>
            <dt>Configured</dt>
            <dd>{{ status?.isConfigured ? 'Yes' : 'No' }}</dd>
          </div>
          <div>
            <dt>Mode</dt>
            <dd>{{ status?.mode ?? 'None' }}</dd>
          </div>
        </dl>
        <p v-if="status && !status.isConfigured" class="azure-table-view__hint">
          Set either AzureTableConnectionString or AzureTableUri in primary-api configuration.
        </p>
      </section>

      <template v-if="status?.isConfigured">
        <section class="azure-table-panel">
          <h3>Tables</h3>
          <ul v-if="tables.length" class="azure-table-list">
            <li v-for="name in tables" :key="name">{{ name }}</li>
          </ul>
          <p v-else class="azure-table-view__hint">No tables found yet.</p>
        </section>

        <section class="azure-table-panel">
          <h3>Table Actions</h3>
          <div class="azure-table-form">
            <label>
              Table name
              <input v-model="tableName" type="text" />
            </label>
            <label>
              Partition key
              <input v-model="partitionKey" type="text" />
            </label>
            <label>
              Row key
              <input v-model="rowKey" type="text" />
            </label>
            <label>
              Message property
              <input v-model="propertyValue" type="text" />
            </label>
          </div>
          <div class="azure-table-actions">
            <button type="button" @click="handleEnsureTable">Ensure Table</button>
            <button type="button" @click="handleUpsertEntity">Upsert Entity</button>
            <button type="button" @click="refreshEntities">Refresh Entities</button>
          </div>
          <p v-if="actionMessage" class="azure-table-view__message">{{ actionMessage }}</p>
        </section>

        <section class="azure-table-panel">
          <h3>Entities in {{ tableName }}</h3>
          <table v-if="entities.length" class="azure-table-entities">
            <thead>
              <tr>
                <th>Partition</th>
                <th>Row</th>
                <th>Properties</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="entity in entities" :key="`${entity.partitionKey}-${entity.rowKey}`">
                <td>{{ entity.partitionKey }}</td>
                <td>{{ entity.rowKey }}</td>
                <td>{{ JSON.stringify(entity.properties) }}</td>
              </tr>
            </tbody>
          </table>
          <p v-else class="azure-table-view__hint">No entities in this table yet.</p>
        </section>
      </template>
    </div>
  </section>
</template>

<style scoped>
.azure-table-view {
  display: grid;
  gap: 1.5rem;
}

.azure-table-view__header h2 {
  margin: 0 0 0.25rem;
}

.azure-table-view__header p,
.azure-table-view__hint,
.azure-table-view__message {
  margin: 0;
  color: #64748b;
}

.azure-table-view__error {
  margin: 0;
  color: #b91c1c;
}

.azure-table-view__content {
  display: grid;
  gap: 1rem;
}

.azure-table-panel {
  border: 1px solid #e2e8f0;
  border-radius: 0.75rem;
  padding: 1rem;
}

.azure-table-panel h3 {
  margin: 0 0 0.75rem;
}

.azure-table-status {
  display: grid;
  gap: 0.5rem;
  margin: 0;
}

.azure-table-status div {
  display: grid;
  grid-template-columns: 8rem 1fr;
  gap: 0.75rem;
}

.azure-table-status dt {
  font-weight: 600;
}

.azure-table-status dd {
  margin: 0;
}

.azure-table-list {
  margin: 0;
  padding-left: 1.25rem;
}

.azure-table-form {
  display: grid;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.azure-table-form label {
  display: grid;
  gap: 0.25rem;
}

.azure-table-form input {
  padding: 0.5rem 0.75rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.5rem;
}

.azure-table-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.azure-table-actions button {
  padding: 0.5rem 0.75rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.5rem;
  background: #f8fafc;
  cursor: pointer;
}

.azure-table-entities {
  width: 100%;
  border-collapse: collapse;
}

.azure-table-entities th,
.azure-table-entities td {
  border-bottom: 1px solid #e2e8f0;
  padding: 0.5rem 0.75rem;
  text-align: left;
  vertical-align: top;
}
</style>
