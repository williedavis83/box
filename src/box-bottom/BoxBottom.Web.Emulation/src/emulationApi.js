export async function fetchFederatedEmulationAnchors() {
  const response = await fetch('/api/meta/emulation/anchors')

  if (!response.ok) {
    throw new Error(`Failed to load emulation diagnostics (${response.status})`)
  }

  return response.json()
}
