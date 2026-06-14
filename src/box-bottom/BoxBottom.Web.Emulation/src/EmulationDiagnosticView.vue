<script setup>
import { onMounted, ref } from 'vue'
import { fetchFederatedEmulationAnchors } from './emulationApi.js'

const data = ref(null)
const error = ref('')
const loading = ref(false)

function activatedCount(anchors) {
  return anchors.filter((anchor) => anchor.isActivated).length
}

async function loadDiagnostics() {
  loading.value = true
  error.value = ''
  data.value = null

  try {
    data.value = await fetchFederatedEmulationAnchors()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load emulation diagnostics'
  } finally {
    loading.value = false
  }
}

onMounted(loadDiagnostics)
</script>

<template>
  <section class="emulation-view">
    <header class="emulation-view__header">
      <h2>Emulation Diagnostics</h2>
      <p>Federated emulation anchor metadata from stack APIs, including which anchors are active for this stack.</p>
    </header>

    <p v-if="loading" data-testid="emulation-loading">Loading...</p>
    <p v-else-if="error" class="emulation-view__error" data-testid="emulation-error">{{ error }}</p>

    <div v-else-if="data" class="emulation-view__groups">
      <section
        v-for="group in data.apis"
        :key="group.apiLogicalName"
        class="emulation-group"
      >
        <div class="emulation-group__header">
          <h3>{{ group.apiLogicalName }}</h3>
          <p
            v-if="!group.error && group.anchors.length"
            class="emulation-group__summary"
            data-testid="emulation-group-summary"
          >
            {{ activatedCount(group.anchors) }} of {{ group.anchors.length }} anchors activated
          </p>
        </div>

        <p v-if="group.error" class="emulation-view__error">{{ group.error }}</p>

        <table v-else-if="group.anchors.length" class="emulation-table">
          <thead>
            <tr>
              <th>Activated</th>
              <th>Assembly</th>
              <th>Anchor</th>
              <th>Service Type</th>
              <th>Kind</th>
              <th>Lifetime</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="anchor in group.anchors"
              :key="`${group.apiLogicalName}-${anchor.anchorName}-${anchor.serviceType}`"
              :class="{ 'emulation-table__row--activated': anchor.isActivated }"
              :data-testid="anchor.isActivated ? 'emulation-anchor-activated' : 'emulation-anchor-inactive'"
            >
              <td>
                <span
                  class="emulation-badge"
                  :class="anchor.isActivated ? 'emulation-badge--active' : 'emulation-badge--inactive'"
                >
                  {{ anchor.isActivated ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td>{{ anchor.assemblyName }}</td>
              <td>{{ anchor.anchorName }}</td>
              <td>{{ anchor.serviceType }}</td>
              <td>{{ anchor.kind }}</td>
              <td>{{ anchor.lifetime }}</td>
            </tr>
          </tbody>
        </table>

        <p v-else class="emulation-view__empty">No emulation anchors registered.</p>
      </section>
    </div>
  </section>
</template>

<style scoped>
.emulation-view {
  display: grid;
  gap: 1.5rem;
}

.emulation-view__header h2 {
  margin: 0 0 0.25rem;
}

.emulation-view__header p,
.emulation-view__empty {
  margin: 0;
  color: #64748b;
}

.emulation-view__error {
  margin: 0;
  color: #b91c1c;
}

.emulation-view__groups {
  display: grid;
  gap: 1.25rem;
}

.emulation-group {
  border: 1px solid #e2e8f0;
  border-radius: 0.75rem;
  padding: 1rem;
}

.emulation-group h3 {
  margin: 0;
}

.emulation-group__header {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  justify-content: space-between;
  gap: 0.5rem 1rem;
  margin-bottom: 0.75rem;
}

.emulation-group__summary {
  margin: 0;
  color: #475569;
  font-size: 0.875rem;
}

.emulation-table__row--activated {
  background: #f0fdf4;
}

.emulation-badge {
  display: inline-block;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.125rem 0.625rem;
}

.emulation-badge--active {
  background: #dcfce7;
  color: #166534;
}

.emulation-badge--inactive {
  background: #f1f5f9;
  color: #64748b;
}

.emulation-table {
  width: 100%;
  border-collapse: collapse;
}

.emulation-table th,
.emulation-table td {
  border-bottom: 1px solid #e2e8f0;
  padding: 0.5rem 0.75rem;
  text-align: left;
  vertical-align: top;
}

.emulation-table th {
  font-size: 0.875rem;
  color: #475569;
}
</style>
