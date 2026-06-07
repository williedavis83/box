import { defineConfig } from '@playwright/test'

const baseURL = process.env.BOX_WEB_HTTP ?? process.env.WEB_HTTP

export default defineConfig({
  testDir: './tests',
  use: {
    baseURL,
  },
})
