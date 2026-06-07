import { test, expect } from '@playwright/test'
import { stackHttp } from './helpers/stack-env.js'

async function expectHello(request, baseUrl, expectedGreeting) {
  test.skip(!baseUrl, `${expectedGreeting} base URL is not configured`)

  const response = await request.get(`${baseUrl}/api/Hello`)
  expect(response.ok()).toBeTruthy()
  expect(await response.text()).toBe(expectedGreeting)
}

const stacks = [
  { name: 'box', greeting: 'Hello, World!' },
  { name: 'bob', greeting: 'Hello, Bob!' },
]

for (const stack of stacks) {
  test(`${stack.name} primary-api returns ${stack.greeting}`, async ({ request }) => {
    await expectHello(request, stackHttp(stack.name, 'primary-api'), stack.greeting)
  })

  test(`${stack.name} edge returns ${stack.greeting}`, async ({ request }) => {
    await expectHello(request, stackHttp(stack.name, 'edge'), stack.greeting)
  })
}

test('box and bob return different greetings in parallel', async ({ request }) => {
  const boxPrimary = stackHttp('box', 'primary-api')
  const bobPrimary = stackHttp('bob', 'primary-api')
  test.skip(!boxPrimary || !bobPrimary, 'box and bob primary-api URLs are not configured')

  const [boxResponse, bobResponse] = await Promise.all([
    request.get(`${boxPrimary}/api/Hello`),
    request.get(`${bobPrimary}/api/Hello`),
  ])

  expect(boxResponse.ok()).toBeTruthy()
  expect(bobResponse.ok()).toBeTruthy()
  expect(await boxResponse.text()).toBe('Hello, World!')
  expect(await bobResponse.text()).toBe('Hello, Bob!')
})
