import axios from 'axios'
import type {
  Period, Transaction, Category, Budget,
  Goal, UpcomingPayment, Card, PaymentMethod, DashboardData
} from '../types'

const api = axios.create({
  baseURL: '/api',
})

// Dashboard
export const getDashboard = (periodId: string) =>
  api.get<DashboardData>(`/dashboard/${periodId}`).then(r => r.data)

// Periods
export const getPeriods        = ()   => api.get<Period[]>('/periods').then(r => r.data)
export const getCurrentPeriod  = ()   => api.get<Period>('/periods/current').then(r => r.data)
export const createPeriod      = (d: Partial<Period>) => api.post<Period>('/periods', d).then(r => r.data)
export const updatePeriod      = (id: string, d: Partial<Period>) => api.put<Period>(`/periods/${id}`, d).then(r => r.data)
export const deletePeriod      = (id: string) => api.delete(`/periods/${id}`)

// Transactions
export const getTransactions = (params?: Record<string, string>) =>
  api.get<Transaction[]>('/transactions', { params }).then(r => r.data)
export const createTransaction = (d: Partial<Transaction>) => api.post<Transaction>('/transactions', d).then(r => r.data)
export const updateTransaction = (id: string, d: Partial<Transaction>) => api.put<Transaction>(`/transactions/${id}`, d).then(r => r.data)
export const deleteTransaction = (id: string) => api.delete(`/transactions/${id}`)

// Categories
export const getCategories    = () => api.get<Category[]>('/categories').then(r => r.data)
export const createCategory   = (d: Partial<Category>) => api.post<Category>('/categories', d).then(r => r.data)
export const updateCategory   = (id: string, d: Partial<Category>) => api.put<Category>(`/categories/${id}`, d).then(r => r.data)
export const deleteCategory   = (id: string) => api.delete(`/categories/${id}`)

// Budgets
export const getBudgets    = (periodId: string) => api.get<Budget[]>(`/budgets/period/${periodId}`).then(r => r.data)
export const upsertBudget  = (d: Partial<Budget>) => api.post<Budget>('/budgets', d).then(r => r.data)
export const deleteBudget  = (id: string) => api.delete(`/budgets/${id}`)

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
export const getCards    = () => api.get<Card[]>('/cards').then(r => r.data)
export const createCard  = (d: Partial<Card>) => api.post<Card>('/cards', d).then(r => r.data)
export const updateCard  = (id: string, d: Partial<Card>) => api.put<Card>(`/cards/${id}`, d).then(r => r.data)
export const deleteCard  = (id: string) => api.delete(`/cards/${id}`)

// Payment methods
export const getPaymentMethods = () => api.get<PaymentMethod[]>('/payment-methods').then(r => r.data)

// Settings
export const getSettings    = () => api.get<Record<string, string>>('/settings').then(r => r.data)
export const updateSetting  = (key: string, value: string) => api.put(`/settings/${key}`, { value })
