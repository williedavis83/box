import { test, expect } from '@playwright/test'
import { stackHttp } from './helpers/stack-env.js'

const stacks = [
  {
    name: 'box',
    ordersMode: 'Uri',
    analyticsMode: 'Uri',
  },
  {
    name: 'bob',
    ordersMode: 'ConnectionString',
    analyticsMode: 'ConnectionString',
  },
]

for (const stack of stacks) {
  test(`${stack.name} primary-api reports azure table scenario modes`, async ({ request }) => {
    const baseUrl = stackHttp(stack.name, 'primary-api')
    test.skip(!baseUrl, `${stack.name} primary-api is not configured`)

    const response = await request.get(`${baseUrl}/api/AzureTable/scenarios`)
    expect(response.ok()).toBeTruthy()

    const scenarios = await response.json()
    expect(scenarios.orders.status.mode).toBe(stack.ordersMode)
    expect(scenarios.analytics.status.mode).toBe(stack.analyticsMode)
    expect(scenarios.geoReplicas).toHaveLength(2)
    expect(scenarios.geoReplicas.every((replica) => replica.status.mode === stack.ordersMode)).toBeTruthy()
  })
}

test('box and bob azure table modes differ in parallel', async ({ request }) => {
  const boxPrimary = stackHttp('box', 'primary-api')
  const bobPrimary = stackHttp('bob', 'primary-api')
  test.skip(!boxPrimary || !bobPrimary, 'box and bob primary-api URLs are not configured')

  const [boxResponse, bobResponse] = await Promise.all([
    request.get(`${boxPrimary}/api/AzureTable/scenarios`),
    request.get(`${bobPrimary}/api/AzureTable/scenarios`),
  ])

  expect(boxResponse.ok()).toBeTruthy()
  expect(bobResponse.ok()).toBeTruthy()

  const boxScenarios = await boxResponse.json()
  const bobScenarios = await bobResponse.json()

  expect(boxScenarios.orders.status.mode).toBe('Uri')
  expect(bobScenarios.orders.status.mode).toBe('ConnectionString')
})

test('bob geo demo ensures and upserts across replicas', async ({ request }) => {
  const bobPrimary = stackHttp('bob', 'primary-api')
  test.skip(!bobPrimary, 'bob primary-api is not configured')

  const response = await request.post(`${bobPrimary}/api/AzureTable/scenarios/geo-demo?message=playwright-geo`)
  expect(response.ok()).toBeTruthy()

  const results = await response.json()
  expect(results).toHaveLength(2)
  expect(results.map((result) => result.region).sort()).toEqual(['eu-west', 'us-east'])
})

test('bob orders account can list tables after geo demo', async ({ request }) => {
  const bobPrimary = stackHttp('bob', 'primary-api')
  test.skip(!bobPrimary, 'bob primary-api is not configured')

  await request.post(`${bobPrimary}/api/AzureTable/scenarios/geo-demo?message=playwright-list`)

  const response = await request.get(`${bobPrimary}/api/AzureTable/accounts/table-orders/tables`)
  expect(response.ok()).toBeTruthy()

  const tables = await response.json()
  expect(Array.isArray(tables)).toBeTruthy()
})
