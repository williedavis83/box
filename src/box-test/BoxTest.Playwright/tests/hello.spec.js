import { test, expect } from '@playwright/test'
import { stackHttp } from './helpers/stack-env.js'

test('shows Hello from the website', async ({ page }) => {
  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'BoxTop.Web' })).toBeVisible()
  await expect(page.getByText('Loading...')).toBeHidden()
  await expect(page.getByText('Hello, World!', { exact: true })).toBeVisible()
})

test('shows Hello Bob from the bob website', async ({ page }) => {
  const baseUrl = stackHttp('bob', 'web')
  test.skip(!baseUrl, 'bob web base URL is not configured')

  await page.goto(baseUrl)
  await expect(page.getByRole('heading', { name: 'BoxTop.Web' })).toBeVisible()
  await expect(page.getByText('Loading...')).toBeHidden()
  await expect(page.getByText('Hello, Bob!', { exact: true })).toBeVisible()
})
