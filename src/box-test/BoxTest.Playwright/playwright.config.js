import { defineConfig } from '@playwright/test'

const baseURL =
  process.env.BOX_WEB_HTTP ??
  process.env.services__web__http__0 ??
  process.env.WEB_HTTP

export default defineConfig({
  testDir: './tests',
  use: {
    baseURL,
  },
})
