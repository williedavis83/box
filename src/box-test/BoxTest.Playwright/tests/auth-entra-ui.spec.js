import { test, expect } from '@playwright/test'
import { stackHttp } from './helpers/stack-env.js'

test.describe('boe real Entra proof', () => {
  test('sign in redirects to ciamlogin.com', async ({ page }) => {
    const boeWeb = stackHttp('boe', 'web')
    test.skip(!boeWeb, 'boe web is not configured')

    await page.goto(boeWeb)

    await page.getByRole('button', { name: 'Sign in' }).click()

    await expect(page).toHaveURL(/ciamlogin\.com/i, { timeout: 30_000 })
  })

  test('completes Entra login and shows authenticated user', async ({ page }) => {
    const boeWeb = stackHttp('boe', 'web')
    const email = process.env.ENTRA_PROOF_EMAIL
    const password = process.env.ENTRA_PROOF_PASSWORD

    test.skip(!boeWeb, 'boe web is not configured')
    test.skip(!email || !password, 'ENTRA_PROOF_EMAIL and ENTRA_PROOF_PASSWORD are not configured')

    await page.goto(boeWeb)

    await page.getByRole('button', { name: 'Sign in' }).click()
    await expect(page).toHaveURL(/ciamlogin\.com/i, { timeout: 30_000 })

    await page.getByLabel(/email/i).fill(email)
    await page.getByRole('button', { name: /next|continue|sign in/i }).click()

    const passwordField = page.getByLabel(/password/i)
    if (await passwordField.isVisible({ timeout: 5_000 }).catch(() => false)) {
      await passwordField.fill(password)
      await page.getByRole('button', { name: /sign in|continue|next/i }).click()
    }

    await expect(page).toHaveURL(new RegExp(`${boeWeb.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`), {
      timeout: 60_000,
    })

    await expect(page.getByRole('button', { name: /^[A-Z?]{1,3}$/ })).toBeVisible({ timeout: 15_000 })
  })
})
