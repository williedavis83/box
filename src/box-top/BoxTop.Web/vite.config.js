import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const repoSrc = path.resolve(__dirname, '../..')

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  define: {
    __BOX_STACK_NAME__: JSON.stringify(process.env.BOX_STACK_NAME ?? ''),
  },
  resolve: {
    alias: {
      vue: path.resolve(__dirname, 'node_modules/vue'),
      'vue-router': path.resolve(__dirname, 'node_modules/vue-router'),
      '@box-bottom/web-components': path.resolve(
        repoSrc,
        'box-bottom/BoxBottom.Web.Components/src/index.js',
      ),
      '@box-pack/web-basics': path.resolve(
        repoSrc,
        'box-pack/BoxPack.Web.Basics/src/index.js',
      ),
      '@box-pack/users-web': path.resolve(
        repoSrc,
        'box-pack/BoxPack.Users.Web/src/index.js',
      ),
    },
  },
  server: {
    port: Number(process.env.PORT ?? 5173),
    strictPort: true,
    fs: {
      allow: [repoSrc],
    },
    proxy: {
      '/api': {
        target: process.env.EDGE_HTTP,
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
