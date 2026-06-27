import { test, expect } from '@playwright/test'
import { stackHttp } from './helpers/stack-env.js'

const stacks = [
  { name: 'box', mode: 'Uri' },
  { name: 'bob', mode: 'ConnectionString' },
]

for (const stack of stacks) {
  test(`${stack.name} users-api reports azure table status`, async ({ request }) => {
    const baseUrl = stackHttp(stack.name, 'users-api')
    test.skip(!baseUrl, `${stack.name} users-api is not configured`)

    const response = await request.get(`${baseUrl}/api/UserProfile/status`)
    expect(response.ok()).toBeTruthy()

    const status = await response.json()
    expect(status.mode).toBe(stack.mode)
    expect(status.isConfigured).toBeTruthy()
  })
}

test('bob users-api can create and read a profile', async ({ request }) => {
  const bobUsers = stackHttp('bob', 'users-api')
  test.skip(!bobUsers, 'bob users-api is not configured')

  const createResponse = await request.post(`${bobUsers}/api/UserProfile`, {
    data: {
      displayName: 'Playwright User',
      email: 'playwright@example.com',
    },
  })

  expect(createResponse.status()).toBe(201)

  const created = await createResponse.json()
  expect(created.userId).toBeTruthy()

  const getResponse = await request.get(`${bobUsers}/api/UserProfile/${created.userId}`)
  expect(getResponse.ok()).toBeTruthy()

  const profile = await getResponse.json()
  expect(profile.displayName).toBe('Playwright User')
  expect(profile.email).toBe('playwright@example.com')
})
