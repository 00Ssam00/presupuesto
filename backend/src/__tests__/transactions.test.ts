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

describe('GET /api/transactions', () => {
  it('lista transacciones sin filtros', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 't1' }] })

    const res = await request(app).get('/api/transactions')

    expect(res.status).toBe(200)
    expect(res.body).toEqual([{ id: 't1' }])
    const [sql, params] = query.mock.calls[0]
    expect(sql).not.toContain('WHERE')
    expect(params).toEqual([])
  })

  it('aplica filtros de period_id, type y category_id combinados', async () => {
    query.mockResolvedValueOnce({ rows: [] })

    await request(app).get('/api/transactions').query({
      period_id: 'p1',
      type: 'expense',
      category_id: 'c1',
    })

    const [sql, params] = query.mock.calls[0]
    expect(sql).toContain('t.period_id = $1')
    expect(sql).toContain('t.type = $2')
    expect(sql).toContain('t.category_id = $3')
    expect(params).toEqual(['p1', 'expense', 'c1'])
  })
})

describe('GET /api/transactions/:id', () => {
  it('devuelve 404 si no existe', async () => {
    query.mockResolvedValueOnce({ rows: [] })
    const res = await request(app).get('/api/transactions/does-not-exist')
    expect(res.status).toBe(404)
  })

  it('devuelve la transacción si existe', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 't1', amount: '100.00' }] })
    const res = await request(app).get('/api/transactions/t1')
    expect(res.status).toBe(200)
    expect(res.body).toEqual({ id: 't1', amount: '100.00' })
  })
})

describe('POST /api/transactions', () => {
  it('crea una transacción y usa null para card_id ausente', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'new-id' }] })

    const res = await request(app).post('/api/transactions').send({
      period_id: 'p1',
      category_id: 'c1',
      payment_method_id: 'pm1',
      type: 'expense',
      amount: 5000,
      description: 'Mercado',
      date: '2026-06-20',
    })

    expect(res.status).toBe(201)
    const [, params] = query.mock.calls[0]
    expect(params).toEqual(['p1', 'c1', 'pm1', null, 'expense', 5000, 'Mercado', null, '2026-06-20'])
  })

  it('propaga errores de base de datos como 500', async () => {
    query.mockRejectedValueOnce(new Error('constraint violation'))

    const res = await request(app).post('/api/transactions').send({
      period_id: 'p1', category_id: 'c1', payment_method_id: 'pm1',
      type: 'expense', amount: 5000, description: 'x', date: '2026-06-20',
    })

    expect(res.status).toBe(500)
    expect(res.body.error).toBe('constraint violation')
  })
})

describe('DELETE /api/transactions/:id', () => {
  it('responde 204', async () => {
    query.mockResolvedValueOnce({ rows: [] })
    const res = await request(app).delete('/api/transactions/t1')
    expect(res.status).toBe(204)
  })
})
