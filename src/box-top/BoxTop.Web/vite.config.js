import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  define: {
    __BOX_STACK_NAME__: JSON.stringify(process.env.BOX_STACK_NAME ?? ''),
  },
  server: {
    port: Number(process.env.PORT ?? 5173),
    strictPort: true,
    proxy: {
      '/api': {
        target: process.env.EDGE_HTTP,
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
