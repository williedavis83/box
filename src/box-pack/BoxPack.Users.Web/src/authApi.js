const usersApiBase = '/api/users'

async function parseJsonResponse(response) {
  if (!response.ok) {
    const message = await response.text()
    throw new Error(message || `Request failed (${response.status})`)
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

export async function fetchAuthConfig() {
  const response = await fetch(`${usersApiBase}/Auth/config`, {
    credentials: 'include',
  })

  return parseJsonResponse(response)
}

export async function fetchAuthSession() {
  const response = await fetch(`${usersApiBase}/Auth/me`, {
    credentials: 'include',
  })

  if (response.status === 401) {
    return null
  }

  return parseJsonResponse(response)
}

export function startEntraLogin(returnUrl = '/') {
  const encodedReturnUrl = encodeURIComponent(returnUrl)
  window.location.href = `${usersApiBase}/Auth/login?returnUrl=${encodedReturnUrl}`
}

export async function zeroAuthLogin(payload) {
  const response = await fetch(`${usersApiBase}/Auth/zero/login`, {
    method: 'POST',
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  return parseJsonResponse(response)
}

export async function logout() {
  const response = await fetch(`${usersApiBase}/Auth/logout`, {
    method: 'POST',
    credentials: 'include',
  })

  return parseJsonResponse(response)
}

export async function fetchMyProfile() {
  const response = await fetch(`${usersApiBase}/UserProfile/me`, {
    credentials: 'include',
  })

  if (response.status === 401) {
    return null
  }

  return parseJsonResponse(response)
}

export async function updateMyProfile(payload) {
  const response = await fetch(`${usersApiBase}/UserProfile/me`, {
    method: 'PUT',
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  return parseJsonResponse(response)
}
