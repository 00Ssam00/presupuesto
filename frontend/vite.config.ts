import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Recursos ya migrados al backend .NET (rutas en español) — ver vercel.json en producción.
      '^/api/(periodos|categorias|tarjetas|metodos-pago|transacciones|presupuestos|panel)': {
        target: 'http://localhost:5080',
        changeOrigin: true,
      },
      // El resto (Goals, UpcomingPayments, Settings) sigue en Node hasta la Fase 7.
      '/api': {
        target: 'http://localhost:3000',
        changeOrigin: true,
      },
    },
  },
})
