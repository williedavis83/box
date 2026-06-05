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
        target: process.env.BOX_EDGE_HTTP,
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
