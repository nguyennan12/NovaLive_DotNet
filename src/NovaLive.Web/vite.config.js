import react from '@vitejs/plugin-react'
import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '~': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    host: true,
    port: 5173,
    allowedHosts: [
      'novalive.online',
      'www.novalive.online'
    ],
    proxy: {
      '/api': {
        target: 'http://localhost:5001',
        changeOrigin: true,
        secure: false
      },
      '/hubs': {
        target: 'http://localhost:5001',
        changeOrigin: true,
        secure: false,
        ws: true
      }
    }
  }
})
