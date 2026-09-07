import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Recursos ya migrados al backend .NET (rutas en español) — ver vercel.json en producción.
      '^/api/(periodos|categorias|tarjetas|metodos-pago|transacciones|presupuestos|panel|metas|pagos-proximos|ajustes)': {
        target: 'http://localhost:5080',
        changeOrigin: true,
      },
      // El resto (nada por ahora — Node queda solo para health-check hasta la Fase 8) sigue en Node.
      '/api': {
        target: 'http://localhost:3000',
        changeOrigin: true,
      },
    },
  },
})
