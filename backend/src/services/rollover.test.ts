import { describe, it, expect, vi, beforeEach } from 'vitest'

vi.mock('../db', () => ({ default: { query: vi.fn() } }))

import pool from '../db'
import { recalculateAllRollovers } from './rollover'

const query = (pool as unknown as { query: ReturnType<typeof vi.fn> }).query

beforeEach(() => {
  query.mockReset()
})

describe('recalculateAllRollovers', () => {
  it('no actualiza nada si los rollovers ya están correctos', async () => {
    query.mockResolvedValueOnce({
      rows: [
        { id: 'p1', start_date: '2026-01-01', base_income: 1000, rollover: 0, total_expenses: 400, total_income_extra: 0 },
        { id: 'p2', start_date: '2026-01-16', base_income: 1000, rollover: 600, total_expenses: 200, total_income_extra: 0 },
      ],
    })

    await recalculateAllRollovers()

    expect(query).toHaveBeenCalledTimes(1)
  })

  it('encadena el sobrante de una quincena como rollover de la siguiente', async () => {
    query.mockResolvedValueOnce({
      rows: [
        { id: 'p1', start_date: '2026-01-01', base_income: 1000, rollover: 0, total_expenses: 400, total_income_extra: 0 },
        { id: 'p2', start_date: '2026-01-16', base_income: 1000, rollover: 0, total_expenses: 200, total_income_extra: 0 },
      ],
    })
    query.mockResolvedValueOnce({ rows: [] })

    await recalculateAllRollovers()

    expect(query).toHaveBeenCalledTimes(2)
    const [sql, params] = query.mock.calls[1]
    expect(sql).toContain('UPDATE periods SET rollover')
    expect(params).toEqual([600, 'p2'])
  })

  it('propaga déficits (rollover negativo) a la siguiente quincena', async () => {
    query.mockResolvedValueOnce({
      rows: [
        { id: 'p1', start_date: '2026-01-01', base_income: 1000, rollover: 0, total_expenses: 1500, total_income_extra: 0 },
        { id: 'p2', start_date: '2026-01-16', base_income: 1000, rollover: 0, total_expenses: 200, total_income_extra: 0 },
      ],
    })
    query.mockResolvedValueOnce({ rows: [] })

    await recalculateAllRollovers()

    const [, params] = query.mock.calls[1]
    expect(params).toEqual([-500, 'p2'])
  })

  it('la primera quincena cronológica siempre queda en rollover 0', async () => {
    query.mockResolvedValueOnce({
      rows: [
        { id: 'p1', start_date: '2026-01-01', base_income: 1000, rollover: 300, total_expenses: 400, total_income_extra: 0 },
      ],
    })
    query.mockResolvedValueOnce({ rows: [] })

    await recalculateAllRollovers()

    const [, params] = query.mock.calls[1]
    expect(params).toEqual([0, 'p1'])
  })
})
