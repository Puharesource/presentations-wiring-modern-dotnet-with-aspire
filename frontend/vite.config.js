import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    host: '0.0.0.0',
    port: Number(process.env.PORT ?? 5173),
    strictPort: true,
    proxy: {
      '/api': {
        target: process.env.BACKEND_ENDPOINT ?? 'http://localhost:5244',
        rewrite: path => path.replace(/^\/api/, ''),
      },
    },
  },
});
