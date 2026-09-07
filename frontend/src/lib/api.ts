import axios from 'axios'
import { showToast } from './toast'
import type {
  Periodo, Transaccion, Categoria, Presupuesto,
  Goal, UpcomingPayment, Tarjeta, MetodoPago, DatosPanel
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
export const getDashboard = (periodId: string) =>
  api.get<DatosPanel>(`/panel/${periodId}`).then(r => r.data)

// Periods
export const getPeriods        = ()   => api.get<Periodo[]>('/periodos').then(r => r.data)
export const getCurrentPeriod  = ()   => api.get<Periodo>('/periodos/actual').then(r => r.data)
export const createPeriod      = (d: Partial<Periodo>) => api.post<Periodo>('/periodos', d).then(r => r.data)
export const updatePeriod      = (id: string, d: Partial<Periodo>) => api.put<Periodo>(`/periodos/${id}`, d).then(r => r.data)
export const deletePeriod      = (id: string) => api.delete(`/periodos/${id}`)

// Transactions
export const getTransactions = (params?: Record<string, string>) =>
  api.get<Transaccion[]>('/transacciones', { params }).then(r => r.data)
export const createTransaction = (d: Partial<Transaccion>) => api.post<Transaccion>('/transacciones', d).then(r => r.data)
export const updateTransaction = (id: string, d: Partial<Transaccion>) => api.put<Transaccion>(`/transacciones/${id}`, d).then(r => r.data)
export const deleteTransaction = (id: string) => api.delete(`/transacciones/${id}`)

export interface ImportRow {
  monto?: string
  tipo?: string
  fecha?: string
  categoria?: string
  metodo?: string
  tarjeta?: string
  descripcion?: string
}
export interface ImportRowResult {
  fila: number
  estado: 'created' | 'error'
  id?: string
  error?: string
}
export interface ImportResponse {
  creados: number
  fallidos: number
  resultados: ImportRowResult[]
}
export const importTransactions = (rows: ImportRow[]) =>
  api.post<ImportResponse>('/transacciones/importar', { filas: rows }).then(r => r.data)

// Categories
export const getCategories    = () => api.get<Categoria[]>('/categorias').then(r => r.data)
export const createCategory   = (d: Partial<Categoria>) => api.post<Categoria>('/categorias', d).then(r => r.data)
export const updateCategory   = (id: string, d: Partial<Categoria>) => api.put<Categoria>(`/categorias/${id}`, d).then(r => r.data)
export const deleteCategory   = (id: string) => api.delete(`/categorias/${id}`)

// Budgets
export const getBudgets    = (periodId: string) => api.get<Presupuesto[]>(`/presupuestos/periodo/${periodId}`).then(r => r.data)
export const upsertBudget  = (d: Partial<Presupuesto>) => api.post<Presupuesto>('/presupuestos', d).then(r => r.data)
export const deleteBudget  = (id: string) => api.delete(`/presupuestos/${id}`)

// Goals
export const getGoals    = () => api.get<Goal[]>('/goals').then(r => r.data)
export const createGoal  = (d: Partial<Goal>) => api.post<Goal>('/goals', d).then(r => r.data)
export const updateGoal  = (id: string, d: Partial<Goal>) => api.put<Goal>(`/goals/${id}`, d).then(r => r.data)
export const deleteGoal  = (id: string) => api.delete(`/goals/${id}`)

// Upcoming payments
export const getUpcomingPayments    = (params?: Record<string, string>) =>
  api.get<UpcomingPayment[]>('/upcoming-payments', { params }).then(r => r.data)
export const createUpcomingPayment  = (d: Partial<UpcomingPayment>) => api.post<UpcomingPayment>('/upcoming-payments', d).then(r => r.data)
export const updateUpcomingPayment  = (id: string, d: Partial<UpcomingPayment>) => api.put<UpcomingPayment>(`/upcoming-payments/${id}`, d).then(r => r.data)
export const deleteUpcomingPayment  = (id: string) => api.delete(`/upcoming-payments/${id}`)

// Cards
export const getCards    = () => api.get<Tarjeta[]>('/tarjetas').then(r => r.data)
export const createCard  = (d: Partial<Tarjeta>) => api.post<Tarjeta>('/tarjetas', d).then(r => r.data)
export const updateCard  = (id: string, d: Partial<Tarjeta>) => api.put<Tarjeta>(`/tarjetas/${id}`, d).then(r => r.data)
export const deleteCard  = (id: string) => api.delete(`/tarjetas/${id}`)

// Payment methods
export const getPaymentMethods = () => api.get<MetodoPago[]>('/metodos-pago').then(r => r.data)

// Settings
export const getSettings    = () => api.get<Record<string, string>>('/settings').then(r => r.data)
export const updateSetting  = (key: string, value: string) => api.put(`/settings/${key}`, { value })
