import { test, expect } from '@playwright/test'
import { stackHttp } from './helpers/stack-env.js'

test('bob users-api zero auth login returns session', async ({ request }) => {
  const bobUsers = stackHttp('bob', 'users-api')
  test.skip(!bobUsers, 'bob users-api is not configured')

  const configResponse = await request.get(`${bobUsers}/api/Auth/config`)
  expect(configResponse.ok()).toBeTruthy()
  const config = await configResponse.json()
  expect(config.provider).toBe('ZeroAuth')

  const loginResponse = await request.post(`${bobUsers}/api/Auth/zero/login`, {
    data: {
      provider: 'ZeroAuth',
      externalId: 'playwright-user',
      displayName: 'Playwright User',
      email: 'playwright@example.com',
    },
  })

  expect(loginResponse.ok()).toBeTruthy()
  const session = await loginResponse.json()
  expect(session.displayName).toBe('Playwright User')
  expect(session.initials).toBe('PU')

  const cookieHeader = loginResponse.headers()['set-cookie']
  expect(cookieHeader).toBeTruthy()

  const meResponse = await request.get(`${bobUsers}/api/Auth/me`, {
    headers: {
      cookie: cookieHeader,
    },
  })

  expect(meResponse.ok()).toBeTruthy()
  const me = await meResponse.json()
  expect(me.userId).toBe(session.userId)

  const profileResponse = await request.get(`${bobUsers}/api/UserProfile/me`, {
    headers: {
      cookie: cookieHeader,
    },
  })

  expect(profileResponse.ok()).toBeTruthy()
  const profile = await profileResponse.json()
  expect(profile.email).toBe('playwright@example.com')

  const logoutResponse = await request.post(`${bobUsers}/api/Auth/logout`, {
    headers: {
      cookie: cookieHeader,
    },
  })

  expect(logoutResponse.status()).toBe(204)
})

test('box users-api rejects zero auth when entra configured', async ({ request }) => {
  const boxUsers = stackHttp('box', 'users-api')
  test.skip(!boxUsers, 'box users-api is not configured')

  const configResponse = await request.get(`${boxUsers}/api/Auth/config`)
  test.skip(!configResponse.ok(), 'box users-api auth is not configured')

  const config = await configResponse.json()
  test.skip(config.provider !== 'Entra', 'box stack is not configured for Entra')

  const loginResponse = await request.post(`${boxUsers}/api/Auth/zero/login`, {
    data: {
      provider: 'ZeroAuth',
      externalId: 'playwright-user',
      displayName: 'Playwright User',
      email: 'playwright@example.com',
    },
  })

  expect(loginResponse.status()).toBe(403)
})
