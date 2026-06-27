import { test, expect } from '@playwright/test'
import { stackHttp } from './helpers/stack-env.js'

test('bob zero auth login shows user menu and profile tab', async ({ page }) => {
  const bobWeb = stackHttp('bob', 'web')
  test.skip(!bobWeb, 'bob web is not configured')

  await page.goto(bobWeb)

  await page.getByRole('button', { name: 'Sign in' }).click()
  await page.getByRole('button', { name: 'Sign in', exact: true }).last().click()

  await expect(page.getByRole('button', { name: 'DU' })).toBeVisible()

  const mainNav = page.getByRole('navigation', { name: 'Main navigation' })
  await expect(mainNav.getByRole('button', { name: 'User Profile', exact: true })).toHaveCount(0)

  await page.getByRole('button', { name: 'DU' }).click()
  await page.getByRole('button', { name: 'User Profile' }).click()

  await expect(page.getByRole('heading', { name: 'User Profile' })).toBeVisible()
  await expect(page.getByDisplayValue('Dev User')).toBeVisible()
  await expect(mainNav.getByRole('button', { name: 'User Profile', exact: true })).toBeVisible()

  await page.getByRole('button', { name: 'Hello', exact: true }).click()
  await expect(mainNav.getByRole('button', { name: 'User Profile', exact: true })).toHaveCount(0)

  await page.getByRole('button', { name: 'DU' }).click()
  await page.getByRole('button', { name: 'Sign out' }).click()

  await expect(page.getByRole('button', { name: 'Sign in' })).toBeVisible()
})
