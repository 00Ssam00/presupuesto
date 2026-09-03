import { describe, it, expect, vi, beforeEach } from 'vitest'
import request from 'supertest'

vi.mock('../db', () => ({ default: { query: vi.fn() } }))

import pool from '../db'
import app from '../app'

const query = (pool as unknown as { query: ReturnType<typeof vi.fn> }).query

beforeEach(() => {
  query.mockReset()
})

describe('GET /api/cards', () => {
  it('solo lista tarjetas activas', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'card1', is_active: true }] })
    const res = await request(app).get('/api/cards')
    expect(res.status).toBe(200)
    const [sql] = query.mock.calls[0]
    expect(sql).toContain('is_active = TRUE')
  })
})

describe('PUT /api/cards/:id', () => {
  it('no envía is_active en el body y no la borra (regresión: is_active pasaba a NULL y la tarjeta desaparecía)', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'card1', name: 'Visa', is_active: true }] })

    const res = await request(app).put('/api/cards/card1').send({
      name: 'Visa', last_four: '1234', bank: 'Bancolombia',
    })

    expect(res.status).toBe(200)
    const [sql, params] = query.mock.calls[0]
    expect(sql).toContain('is_active=COALESCE($4, is_active)')
    expect(params).toEqual(['Visa', '1234', 'Bancolombia', undefined, 'card1'])
  })

  it('permite desactivar explícitamente enviando is_active: false', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'card1', is_active: false }] })

    await request(app).put('/api/cards/card1').send({
      name: 'Visa', is_active: false,
    })

    const [, params] = query.mock.calls[0]
    expect(params[3]).toBe(false)
  })
})

describe('DELETE /api/cards/:id', () => {
  it('hace soft-delete (is_active = FALSE)', async () => {
    query.mockResolvedValueOnce({ rows: [] })
    const res = await request(app).delete('/api/cards/card1')
    expect(res.status).toBe(204)
    const [sql] = query.mock.calls[0]
    expect(sql).toContain('is_active = FALSE')
  })
})
