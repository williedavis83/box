import { test, expect } from '@playwright/test'

test('shows Hello from the website', async ({ page }) => {
  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'BoxTop.Web' })).toBeVisible()
  await expect(page.getByText('Hello', { exact: true })).toBeVisible()
})
