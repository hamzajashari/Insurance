import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/Claims': { target: 'https://localhost:7052', secure: false },
      '/Covers': { target: 'https://localhost:7052', secure: false },
    },
  },
})
