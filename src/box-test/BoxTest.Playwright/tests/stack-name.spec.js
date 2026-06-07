import { test, expect } from '@playwright/test'
import { stackHttp } from './helpers/stack-env.js'

const boxResources = [
  { name: 'web', baseUrl: stackHttp('box', 'web'), kind: 'page' },
  { name: 'edge', baseUrl: stackHttp('box', 'edge'), kind: 'api' },
  { name: 'primary-api', baseUrl: stackHttp('box', 'primary-api'), kind: 'api' },
  { name: 'secondary-api', baseUrl: stackHttp('box', 'secondary-api'), kind: 'api' },
]

const bobResources = [
  { name: 'web', baseUrl: stackHttp('bob', 'web'), kind: 'page' },
  { name: 'edge', baseUrl: stackHttp('bob', 'edge'), kind: 'api' },
  { name: 'primary-api', baseUrl: stackHttp('bob', 'primary-api'), kind: 'api' },
  { name: 'secondary-api', baseUrl: stackHttp('bob', 'secondary-api'), kind: 'api' },
]

async function expectStackNameApi(request, baseUrl, expectedStackName) {
  test.skip(!baseUrl, `${expectedStackName} API base URL is not configured`)

  const response = await request.get(`${baseUrl}/api/StackName`)
  expect(response.ok()).toBeTruthy()
  expect((await response.json()).stackName).toBe(expectedStackName)
}

for (const resource of boxResources) {
  test(`box ${resource.name} reports stack name box`, async ({ page, request }) => {
    test.skip(!resource.baseUrl, `box ${resource.name} base URL is not configured`)

    if (resource.kind === 'page') {
      await page.goto(resource.baseUrl)
      await expect(page.locator('[data-stack-name]')).toHaveText('box')
      return
    }

    await expectStackNameApi(request, resource.baseUrl, 'box')
  })
}

for (const resource of bobResources) {
  test(`bob ${resource.name} reports stack name bob`, async ({ page, request }) => {
    test.skip(!resource.baseUrl, `bob ${resource.name} base URL is not configured`)

    if (resource.kind === 'page') {
      await page.goto(resource.baseUrl)
      await expect(page.locator('[data-stack-name]')).toHaveText('bob')
      return
    }

    await expectStackNameApi(request, resource.baseUrl, 'bob')
  })
}
