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

describe('GET /api/periods/current', () => {
  it('devuelve el período cuyo rango cubre la fecha de hoy', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'today-period' }] })

    const res = await request(app).get('/api/periods/current')

    expect(res.status).toBe(200)
    expect(res.body).toEqual({ id: 'today-period' })
  })

  it('cae al período más reciente si ninguno cubre hoy', async () => {
    query
      .mockResolvedValueOnce({ rows: [] })
      .mockResolvedValueOnce({ rows: [{ id: 'latest-period' }] })

    const res = await request(app).get('/api/periods/current')

    expect(res.status).toBe(200)
    expect(res.body).toEqual({ id: 'latest-period' })
  })

  it('devuelve null si no hay ningún período en la base de datos', async () => {
    query.mockResolvedValueOnce({ rows: [] }).mockResolvedValueOnce({ rows: [] })

    const res = await request(app).get('/api/periods/current')

    expect(res.status).toBe(200)
    expect(res.body).toBeNull()
  })
})

describe('GET /api/periods/:id', () => {
  it('no es interceptada por la ruta /current gracias al orden de registro', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'abc-123' }] })
    const res = await request(app).get('/api/periods/abc-123')
    expect(res.status).toBe(200)
    expect(res.body.id).toBe('abc-123')
  })

  it('devuelve 404 si el período no existe', async () => {
    query.mockResolvedValueOnce({ rows: [] })
    const res = await request(app).get('/api/periods/no-existe')
    expect(res.status).toBe(404)
  })
})

describe('POST /api/periods', () => {
  it('usa 0 por defecto para base_income y rollover ausentes', async () => {
    query.mockResolvedValueOnce({ rows: [{ id: 'new' }] })

    await request(app).post('/api/periods').send({
      label: 'Test', start_date: '2026-01-01', end_date: '2026-01-15',
    })

    const [, params] = query.mock.calls[0]
    expect(params).toEqual(['Test', '2026-01-01', '2026-01-15', 0, 0])
  })
})
