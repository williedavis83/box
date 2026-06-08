import { test, expect } from '@playwright/test'
import { stackHttp } from './helpers/stack-env.js'

test.describe('tab menu', () => {
  test('test tab is visible in dev and can add overflow tabs', async ({ page }) => {
    const baseUrl = stackHttp('box', 'web') ?? '/'
    await page.setViewportSize({ width: 640, height: 720 })
    await page.goto(`${baseUrl.replace(/\/$/, '')}/test`)

    await expect(page.getByRole('button', { name: 'Test', exact: true })).toBeVisible()
    await expect(page.getByTestId('add-tab-button')).toBeVisible()

    for (let index = 0; index < 8; index += 1) {
      await page.getByTestId('add-tab-button').click()
    }

    const moreButton = page.getByTestId('tab-menu-more')
    await expect(moreButton).toBeVisible()

    await moreButton.click()
    await page.getByRole('button', { name: 'Extra 8', exact: true }).click()

    await expect(page.getByTestId('extra-tab-content')).toBeVisible()
    await expect(page.getByRole('heading', { name: 'Extra tab 8' })).toBeVisible()
  })

  test('hello tab remains reachable from tab bar', async ({ page }) => {
    await page.goto('/test')
    await page.getByRole('button', { name: 'Hello', exact: true }).click()
    await expect(page.getByText('Hello, World!', { exact: true })).toBeVisible()
  })
})
