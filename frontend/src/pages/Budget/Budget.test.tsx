import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Budget from './index'
import { getBudgets, upsertBudget } from '../../lib/api'
import { useReferenceData } from '../../lib/ReferenceDataContext'

vi.mock('../../lib/api')
vi.mock('../../lib/ReferenceDataContext')

const period = {
  id: 'p1', label: 'Quincena de prueba', start_date: '2026-01-01', end_date: '2026-01-15',
  base_income: 1000000, rollover: 0, is_closed: false, created_at: '2026-01-01T00:00:00.000Z',
}

beforeEach(() => {
  vi.mocked(useReferenceData).mockReturnValue({
    categories: [], methods: [], cards: [],
    periods: [period],
    currentPeriod: period,
    loading: false,
    error: '',
    refresh: vi.fn(),
  })
})

describe('Budget', () => {
  it('muestra las categorías sin presupuesto asignado con monto en 0, listas para editar', async () => {
    vi.mocked(getBudgets).mockResolvedValue([
      { id: 'b1', period_id: 'p1', category_id: 'c1', amount: 50000, percentage: null, category_name: 'Alimentación', category_color: '#000', spent: 20000 },
      { id: null, period_id: 'p1', category_id: 'c2', amount: 0, percentage: null, category_name: 'Transporte', category_color: '#111', spent: 0 },
    ])

    render(<Budget />)

    await waitFor(() => expect(screen.getByText('Transporte')).toBeInTheDocument())
    expect(screen.getByText('Alimentación')).toBeInTheDocument()

    const inputs = screen.getAllByRole('spinbutton') as HTMLInputElement[]
    expect(inputs.map(i => i.value)).toEqual(['50000', '0'])
  })

  it('asigna presupuesto a una categoría sin fila existente (upsert por category_id)', async () => {
    vi.mocked(getBudgets).mockResolvedValue([
      { id: null, period_id: 'p1', category_id: 'c2', amount: 0, percentage: null, category_name: 'Transporte', category_color: '#111', spent: 0 },
    ])
    vi.mocked(upsertBudget).mockResolvedValue({ id: 'new-b', period_id: 'p1', category_id: 'c2', amount: 80000, percentage: null })

    render(<Budget />)
    await waitFor(() => expect(screen.getByText('Transporte')).toBeInTheDocument())

    const input = screen.getByRole('spinbutton')
    await userEvent.clear(input)
    await userEvent.type(input, '80000')
    await userEvent.tab()

    await waitFor(() => {
      expect(upsertBudget).toHaveBeenCalledWith({ period_id: 'p1', category_id: 'c2', amount: 80000 })
    })
  })
})
