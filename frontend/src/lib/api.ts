import axios from 'axios'
import { showToast } from './toast'
import type {
  Periodo, Transaccion, Categoria, Presupuesto,
  Meta, PagoProximo, Tarjeta, MetodoPago, DatosPanel
} from '../types'

const api = axios.create({
  baseURL: '/api',
})

// Las consultas típicas al backend tardan ~0.5-0.75s (la más pesada, el
// dashboard, tras paralelizar sus queries). Si una solicitud sigue pendiente
// al triple de ese tiempo (~2s, redondeado), avisamos con un toast en vez de
// dejar al usuario mirando un skeleton indefinidamente sin explicación.
const SLOW_REQUEST_MS = 2000
const pendingSlowTimers = new WeakMap<object, ReturnType<typeof setTimeout>>()

api.interceptors.request.use(config => {
  const timer = setTimeout(() => {
    showToast('La conexión está lenta. Seguimos esperando la respuesta del servidor…', 'error')
  }, SLOW_REQUEST_MS)
  pendingSlowTimers.set(config, timer)
  return config
})

api.interceptors.response.use(
  response => {
    const timer = pendingSlowTimers.get(response.config)
    if (timer) clearTimeout(timer)
    return response
  },
  error => {
    if (error.config) {
      const timer = pendingSlowTimers.get(error.config)
      if (timer) clearTimeout(timer)
    }
    return Promise.reject(error)
  }
)

// Dashboard
export const obtenerPanel = (periodId: string) =>
  api.get<DatosPanel>(`/panel/${periodId}`).then(r => r.data)

// Periods
export const obtenerPeriodos        = ()   => api.get<Periodo[]>('/periodos').then(r => r.data)
export const obtenerPeriodoActual  = ()   => api.get<Periodo>('/periodos/actual').then(r => r.data)
export const crearPeriodo      = (d: Partial<Periodo>) => api.post<Periodo>('/periodos', d).then(r => r.data)
export const actualizarPeriodo      = (id: string, d: Partial<Periodo>) => api.put<Periodo>(`/periodos/${id}`, d).then(r => r.data)
export const eliminarPeriodo      = (id: string) => api.delete(`/periodos/${id}`)

// Transactions
export const obtenerTransacciones = (params?: Record<string, string>) =>
  api.get<Transaccion[]>('/transacciones', { params }).then(r => r.data)
export const crearTransaccion = (d: Partial<Transaccion>) => api.post<Transaccion>('/transacciones', d).then(r => r.data)
export const actualizarTransaccion = (id: string, d: Partial<Transaccion>) => api.put<Transaccion>(`/transacciones/${id}`, d).then(r => r.data)
export const eliminarTransaccion = (id: string) => api.delete(`/transacciones/${id}`)

export interface FilaImportacion {
  monto?: string
  tipo?: string
  fecha?: string
  categoria?: string
  metodo?: string
  tarjeta?: string
  descripcion?: string
}
export interface ResultadoFilaImportacion {
  fila: number
  estado: 'created' | 'error'
  id?: string
  error?: string
}
export interface RespuestaImportacion {
  creados: number
  fallidos: number
  resultados: ResultadoFilaImportacion[]
}
export const importarTransacciones = (rows: FilaImportacion[]) =>
  api.post<RespuestaImportacion>('/transacciones/importar', { filas: rows }).then(r => r.data)

// Categories
export const obtenerCategorias    = () => api.get<Categoria[]>('/categorias').then(r => r.data)
export const crearCategoria   = (d: Partial<Categoria>) => api.post<Categoria>('/categorias', d).then(r => r.data)
export const actualizarCategoria   = (id: string, d: Partial<Categoria>) => api.put<Categoria>(`/categorias/${id}`, d).then(r => r.data)
export const eliminarCategoria   = (id: string) => api.delete(`/categorias/${id}`)

// Budgets
export const obtenerPresupuestos    = (periodId: string) => api.get<Presupuesto[]>(`/presupuestos/periodo/${periodId}`).then(r => r.data)
export const guardarPresupuesto  = (d: Partial<Presupuesto>) => api.post<Presupuesto>('/presupuestos', d).then(r => r.data)
export const eliminarPresupuesto  = (id: string) => api.delete(`/presupuestos/${id}`)

// Goals (Fase 7 — ya en .NET, rutas y campos en español)
export const obtenerMetas    = () => api.get<Meta[]>('/metas').then(r => r.data)
export const crearMeta  = (d: Partial<Meta>) => api.post<Meta>('/metas', d).then(r => r.data)
export const actualizarMeta  = (id: string, d: Partial<Meta>) => api.put<Meta>(`/metas/${id}`, d).then(r => r.data)
export const eliminarMeta  = (id: string) => api.delete(`/metas/${id}`)

// Upcoming payments (Fase 7 — ya en .NET, rutas y campos en español)
export const obtenerPagosProximos    = (params?: Record<string, string>) =>
  api.get<PagoProximo[]>('/pagos-proximos', { params }).then(r => r.data)
export const crearPagoProximo  = (d: Partial<PagoProximo>) => api.post<PagoProximo>('/pagos-proximos', d).then(r => r.data)
export const actualizarPagoProximo  = (id: string, d: Partial<PagoProximo>) => api.put<PagoProximo>(`/pagos-proximos/${id}`, d).then(r => r.data)
export const eliminarPagoProximo  = (id: string) => api.delete(`/pagos-proximos/${id}`)

// Cards
export const obtenerTarjetas    = () => api.get<Tarjeta[]>('/tarjetas').then(r => r.data)
export const crearTarjeta  = (d: Partial<Tarjeta>) => api.post<Tarjeta>('/tarjetas', d).then(r => r.data)
export const actualizarTarjeta  = (id: string, d: Partial<Tarjeta>) => api.put<Tarjeta>(`/tarjetas/${id}`, d).then(r => r.data)
export const eliminarTarjeta  = (id: string) => api.delete(`/tarjetas/${id}`)

// Payment methods
export const obtenerMetodosPago = () => api.get<MetodoPago[]>('/metodos-pago').then(r => r.data)

// Settings (Fase 7 — ya en .NET, rutas y campos en español)
export const obtenerAjustes    = () => api.get<Record<string, string>>('/ajustes').then(r => r.data)
export const actualizarAjuste  = (key: string, value: string) => api.put(`/ajustes/${key}`, { valor: value })
