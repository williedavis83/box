export async function fetchAzureTableStatus() {
  const response = await fetch('/api/AzureTable/status')
  if (!response.ok) {
    throw new Error(`Failed to load Azure Table status (${response.status})`)
  }

  return response.json()
}

export async function fetchAzureTables() {
  const response = await fetch('/api/AzureTable/tables')
  if (!response.ok) {
    throw new Error(`Failed to load Azure tables (${response.status})`)
  }

  return response.json()
}

export async function fetchAzureTableEntities(tableName, maxResults = 25) {
  const response = await fetch(
    `/api/AzureTable/tables/${encodeURIComponent(tableName)}/entities?maxResults=${maxResults}`,
  )

  if (!response.ok) {
    throw new Error(`Failed to load Azure Table entities (${response.status})`)
  }

  return response.json()
}

export async function ensureAzureTable(tableName) {
  const response = await fetch(`/api/AzureTable/tables/${encodeURIComponent(tableName)}`, {
    method: 'POST',
  })

  if (!response.ok) {
    throw new Error(`Failed to ensure Azure table (${response.status})`)
  }
}

export async function upsertAzureTableEntity(tableName, entity) {
  const response = await fetch(`/api/AzureTable/tables/${encodeURIComponent(tableName)}/entities`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(entity),
  })

  if (!response.ok) {
    throw new Error(`Failed to upsert Azure Table entity (${response.status})`)
  }
}
