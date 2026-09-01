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

describe('POST /api/budgets (upsert)', () => {
  it('hace ON CONFLICT DO UPDATE por period_id + category_id', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'b1', amount: '100000' }] })

    const res = await request(app).post('/api/budgets').send({
      period_id: 'p1', category_id: 'c1', amount: 100000,
    })

    expect(res.status).toBe(201)
    const [sql, params] = query.mock.calls[0]
    expect(sql).toContain('ON CONFLICT (period_id, category_id)')
    expect(params).toEqual(['p1', 'c1', 100000, null])
  })
})

describe('GET /api/budgets/period/:periodId', () => {
  it('devuelve budgets con el monto gastado calculado', async () => {
    query.mockResolvedValueOnce({
      rows: [{ id: 'b1', category_id: 'c1', amount: '50000', spent: '20000' }],
    })

    const res = await request(app).get('/api/budgets/period/p1')

    expect(res.status).toBe(200)
    expect(res.body).toEqual([{ id: 'b1', category_id: 'c1', amount: '50000', spent: '20000' }])
  })
})
