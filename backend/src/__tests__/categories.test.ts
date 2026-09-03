import { describe, it, expect, vi, beforeEach } from 'vitest'
import request from 'supertest'

vi.mock('../db', () => ({ default: { query: vi.fn() } }))

import pool from '../db'
import app from '../app'

const query = (pool as unknown as { query: ReturnType<typeof vi.fn> }).query

beforeEach(() => {
  query.mockReset()
})

describe('GET /api/categories', () => {
  it('solo lista categorías activas', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'c1', is_active: true }] })
    const res = await request(app).get('/api/categories')
    expect(res.status).toBe(200)
    const [sql] = query.mock.calls[0]
    expect(sql).toContain('is_active = TRUE')
  })
})

describe('PUT /api/categories/:id', () => {
  it('no envía is_active/icon/sort_order en el body y no los borra (regresión: is_active pasaba a NULL y la categoría desaparecía)', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'c1', name: 'Nueva', is_active: true }] })

    const res = await request(app).put('/api/categories/c1').send({
      name: 'Nueva', color: '#111111',
    })

    expect(res.status).toBe(200)
    const [sql, params] = query.mock.calls[0]
    expect(sql).toContain('is_active=COALESCE($4, is_active)')
    expect(sql).toContain('icon=COALESCE($3, icon)')
    expect(sql).toContain('sort_order=COALESCE($5, sort_order)')
    expect(params).toEqual(['Nueva', '#111111', undefined, undefined, undefined, 'c1'])
  })

  it('permite desactivar explícitamente enviando is_active: false', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'c1', is_active: false }] })

    await request(app).put('/api/categories/c1').send({
      name: 'Nueva', color: '#111111', is_active: false,
    })

    const [, params] = query.mock.calls[0]
    expect(params[3]).toBe(false)
  })
})

describe('DELETE /api/categories/:id', () => {
  it('hace soft-delete (is_active = FALSE)', async () => {
    query.mockResolvedValueOnce({ rows: [] })
    const res = await request(app).delete('/api/categories/c1')
    expect(res.status).toBe(204)
    const [sql] = query.mock.calls[0]
    expect(sql).toContain('is_active = FALSE')
  })
})
