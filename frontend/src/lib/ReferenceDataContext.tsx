import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { getCategories, getPaymentMethods, getCards, getPeriods, getCurrentPeriod } from './api'
import type { Category, PaymentMethod, Card, Period } from '../types'

interface ReferenceData {
  categories: Category[]
  methods: PaymentMethod[]
  cards: Card[]
  periods: Period[]
  currentPeriod: Period | null
  loading: boolean
  error: string
  refresh: () => Promise<void>
}

const ReferenceDataContext = createContext<ReferenceData | null>(null)

// Categorías, métodos de pago, tarjetas, quincenas y la quincena actual cambian
// muy poco (solo desde Configuración) — se cargan una sola vez por sesión en
// vez de que cada pantalla las vuelva a pedir al servidor en cada navegación.
export function ReferenceDataProvider({ children }: { children: ReactNode }) {
  const [categories, setCategories] = useState<Category[]>([])
  const [methods, setMethods] = useState<PaymentMethod[]>([])
  const [cards, setCards] = useState<Card[]>([])
  const [periods, setPeriods] = useState<Period[]>([])
  const [currentPeriod, setCurrentPeriod] = useState<Period | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = useCallback(() => {
    return Promise.all([
      getCategories(),
      getPaymentMethods(),
      getCards(),
      getPeriods(),
      getCurrentPeriod().catch(() => null),
    ]).then(([cats, meths, cds, pds, current]) => {
      setCategories(cats)
      setMethods(meths)
      setCards(cds)
      setPeriods(pds)
      setCurrentPeriod(current)
      setError('')
    }).catch(() => {
      setError('No se pudo conectar al servidor.')
    })
  }, [])

  useEffect(() => {
    load().finally(() => setLoading(false))
  }, [load])

  const refresh = useCallback(() => load(), [load])

  const value = useMemo(
    () => ({ categories, methods, cards, periods, currentPeriod, loading, error, refresh }),
    [categories, methods, cards, periods, currentPeriod, loading, error, refresh]
  )

  return <ReferenceDataContext.Provider value={value}>{children}</ReferenceDataContext.Provider>
}

export function useReferenceData() {
  const ctx = useContext(ReferenceDataContext)
  if (!ctx) throw new Error('useReferenceData debe usarse dentro de ReferenceDataProvider')
  return ctx
}
