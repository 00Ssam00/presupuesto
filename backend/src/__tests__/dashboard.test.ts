import { describe, it, expect, vi, beforeEach } from 'vitest'
import request from 'supertest'

vi.mock('../db', () => ({
  default: { query: vi.fn() },
}))

import pool from '../db'
import app from '../app'

const query = (pool as unknown as { query: ReturnType<typeof vi.fn> }).query

beforeEach(() => {
  query.mockReset()
})

describe('GET /api/dashboard/:periodId', () => {
  it('devuelve 404 si el período no existe', async () => {
    query.mockResolvedValueOnce({ rows: [] })

    const res = await request(app).get('/api/dashboard/no-existe')

    expect(res.status).toBe(404)
    expect(res.body.error).toBe('Período no encontrado')
  })

  it('calcula available y remaining a partir de ingreso base, rollover y gastos', async () => {
    query
      .mockResolvedValueOnce({
        rows: [{ id: 'p1', base_income: '1000000.00', rollover: '50000.00' }],
      })
      .mockResolvedValueOnce({
        rows: [{ total_expenses: '300000.00', total_income_extra: '0.00' }],
      })
      .mockResolvedValueOnce({ rows: [] })
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).get('/api/dashboard/p1')

    expect(res.status).toBe(200)
    // available = base_income + rollover + total_income_extra
    expect(res.body.available).toBe(1050000)
    // remaining = available - total_expenses
    expect(res.body.remaining).toBe(750000)
    expect(res.body.total_expenses).toBe(300000)
  })

  it('los montos NUMERIC de Postgres (string) no producen NaN ni concatenación', async () => {
    query
      .mockResolvedValueOnce({
        rows: [{ id: 'p1', base_income: '200.00', rollover: '0.00' }],
      })
      .mockResolvedValueOnce({
        rows: [{ total_expenses: '50.00', total_income_extra: '10.00' }],
      })
      .mockResolvedValueOnce({ rows: [] })
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).get('/api/dashboard/p1')

    // Si el controller concatenara strings en vez de sumar, available sería "20010" en vez de 210.
    expect(res.body.available).toBe(210)
    expect(res.body.remaining).toBe(160)
  })
})
