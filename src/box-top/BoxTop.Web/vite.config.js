import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    port: Number(process.env.PORT ?? 5173),
    strictPort: true,
    proxy: {
      '/api': {
        target: process.env.PRIMARY_API_HTTPS ?? process.env.PRIMARY_API_HTTP,
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
