import react from '@vitejs/plugin-react';
import path from 'path';
import { defineConfig } from 'vite';

// https://vite.dev/config/
export default defineConfig({
  base: '/DigitalPrize/',
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 3000,
    host: '127.0.0.1',
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      '/.well-known': {
        target: 'https://worldplayauth.ngrok.app',
        changeOrigin: true,
        secure: true,
      },
      '/connect': {
        target: 'https://worldplayauth.ngrok.app',
        changeOrigin: true,
        secure: true,
      },
      '/auth-api': {
        target: 'https://worldplayauth.ngrok.app',
        changeOrigin: true,
        secure: true,
        rewrite: (path) => path.replace(/^\/auth-api/, '/api'),
      },
    },
  },
});
