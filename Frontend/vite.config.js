import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      '/url/shorten': {
        target: 'http://localhost:5001',
        changeOrigin: true,
      },
    },
  },
})
