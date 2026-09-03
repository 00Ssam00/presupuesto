import { describe, it, expect, vi, beforeEach } from 'vitest'
import request from 'supertest'

vi.mock('../db', () => ({ default: { query: vi.fn() } }))

import pool from '../db'
import app from '../app'

const query = (pool as unknown as { query: ReturnType<typeof vi.fn> }).query

beforeEach(() => {
  query.mockReset()
})

describe('GET /api/goals', () => {
  it('solo lista metas activas', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'g1', is_active: true }] })
    const res = await request(app).get('/api/goals')
    expect(res.status).toBe(200)
    const [sql] = query.mock.calls[0]
    expect(sql).toContain('is_active = TRUE')
  })
})

describe('PUT /api/goals/:id', () => {
  it('no envía is_active en el body y no la borra (regresión: is_active pasaba a NULL y la meta desaparecía)', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'g1', name: 'Vacaciones', is_active: true }] })

    const res = await request(app).put('/api/goals/g1').send({
      name: 'Vacaciones', type: 'savings', target_amount: 1000000, current_amount: 200000,
    })

    expect(res.status).toBe(200)
    const [sql, params] = query.mock.calls[0]
    expect(sql).toContain('is_active=COALESCE($6, is_active)')
    expect(params).toEqual(['Vacaciones', 'savings', 1000000, 200000, null, undefined, 'g1'])
  })

  it('permite desactivar explícitamente enviando is_active: false', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'g1', is_active: false }] })

    await request(app).put('/api/goals/g1').send({
      name: 'Vacaciones', type: 'savings', target_amount: 1000000, current_amount: 200000, is_active: false,
    })

    const [, params] = query.mock.calls[0]
    expect(params[5]).toBe(false)
  })
})

describe('DELETE /api/goals/:id', () => {
  it('hace soft-delete (is_active = FALSE)', async () => {
    query.mockResolvedValueOnce({ rows: [] })
    const res = await request(app).delete('/api/goals/g1')
    expect(res.status).toBe(204)
    const [sql] = query.mock.calls[0]
    expect(sql).toContain('is_active = FALSE')
  })
})
