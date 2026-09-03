import { describe, it, expect, vi, beforeEach } from 'vitest'
import request from 'supertest'

vi.mock('../db', () => ({
  default: { query: vi.fn() },
}))

import pool from '../db'
import app from '../app'

const query = (pool as unknown as { query: ReturnType<typeof vi.fn> }).query

const PERIOD = { id: 'p1', start_date: '2026-06-16', end_date: '2026-06-30' }
const CATEGORIES = [{ id: 'c1', name: 'Alimentación' }]
const METHODS_NO_CARD = [{ id: 'pm1', name: 'Efectivo', requires_card: false }]
const METHODS_CARD = [{ id: 'pm2', name: 'Crédito', requires_card: true }]
const CARDS = [{ id: 'card1', name: 'Visa Gold' }]

beforeEach(() => {
  query.mockReset()
})

describe('POST /api/transactions/import', () => {
  it('rechaza sin period_id ni rows', async () => {
    const res = await request(app).post('/api/transactions/import').send({})
    expect(res.status).toBe(400)
    expect(query).not.toHaveBeenCalled()
  })

  it('devuelve 404 si la quincena no existe', async () => {
    query.mockResolvedValueOnce({ rows: [] })
    const res = await request(app).post('/api/transactions/import').send({
      period_id: 'no-existe', rows: [{ monto: '1000', tipo: 'expense' }],
    })
    expect(res.status).toBe(404)
  })

  it('importa una fila válida y recalcula los rollovers', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD] }) // SELECT period
      .mockResolvedValueOnce({ rows: CATEGORIES }) // SELECT categories
      .mockResolvedValueOnce({ rows: METHODS_NO_CARD }) // SELECT payment_methods
      .mockResolvedValueOnce({ rows: [] }) // SELECT cards
      .mockResolvedValueOnce({ rows: [{ id: 'new-tx' }] }) // INSERT
      .mockResolvedValueOnce({ rows: [] }) // SELECT de recalculateAllRollovers

    const res = await request(app).post('/api/transactions/import').send({
      period_id: 'p1',
      rows: [{ monto: '85000', tipo: 'expense', fecha: '2026-06-20', categoria: 'Alimentación', metodo: 'Efectivo', descripcion: 'Mercado' }],
    })

    expect(res.status).toBe(200)
    expect(res.body).toEqual({
      created: 1, failed: 0,
      results: [{ row: 1, status: 'created', id: 'new-tx' }],
    })
  })

  it('reporta por fila las categorías/métodos inexistentes sin bloquear el resto (importación parcial)', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_NO_CARD })
      .mockResolvedValueOnce({ rows: [] })
      .mockResolvedValueOnce({ rows: [{ id: 'new-tx' }] }) // solo la fila válida se inserta
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).post('/api/transactions/import').send({
      period_id: 'p1',
      rows: [
        { monto: '85000', tipo: 'expense', fecha: '2026-06-20', categoria: 'Categoría Inexistente', metodo: 'Efectivo' },
        { monto: '20000', tipo: 'expense', fecha: '2026-06-20', categoria: 'Alimentación', metodo: 'Efectivo' },
      ],
    })

    expect(res.status).toBe(200)
    expect(res.body.created).toBe(1)
    expect(res.body.failed).toBe(1)
    expect(res.body.results[0]).toEqual({ row: 1, status: 'error', error: expect.stringContaining('Categoría Inexistente') })
    expect(res.body.results[1]).toEqual({ row: 2, status: 'created', id: 'new-tx' })
  })

  it('rechaza filas cuya fecha cae fuera del rango de la quincena', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_NO_CARD })
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).post('/api/transactions/import').send({
      period_id: 'p1',
      rows: [{ monto: '85000', tipo: 'expense', fecha: '2026-07-05', categoria: 'Alimentación', metodo: 'Efectivo' }],
    })

    expect(res.status).toBe(200)
    expect(res.body.created).toBe(0)
    expect(res.body.failed).toBe(1)
    expect(res.body.results[0].error).toContain('fuera del rango')
    // No se llama a recalculateAllRollovers si no se creó nada.
    expect(query).toHaveBeenCalledTimes(4)
  })

  it('usa la fecha de inicio de la quincena cuando la fila no trae fecha', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_NO_CARD })
      .mockResolvedValueOnce({ rows: [] })
      .mockResolvedValueOnce({ rows: [{ id: 'new-tx' }] })
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).post('/api/transactions/import').send({
      period_id: 'p1',
      rows: [{ monto: '85000', tipo: 'expense', categoria: 'Alimentación', metodo: 'Efectivo' }],
    })

    expect(res.status).toBe(200)
    const insertCall = query.mock.calls[4]
    expect(insertCall[1]).toContain(PERIOD.start_date)
  })

  it('exige tarjeta cuando el método de pago la requiere', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_CARD })
      .mockResolvedValueOnce({ rows: [] }) // sin tarjetas activas

    const res = await request(app).post('/api/transactions/import').send({
      period_id: 'p1',
      rows: [{ monto: '85000', tipo: 'expense', fecha: '2026-06-20', categoria: 'Alimentación', metodo: 'Crédito', tarjeta: 'Visa Gold' }],
    })

    expect(res.status).toBe(200)
    expect(res.body.failed).toBe(1)
    expect(res.body.results[0].error).toContain('requiere tarjeta')
  })

  it('resuelve la tarjeta correctamente cuando existe', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_CARD })
      .mockResolvedValueOnce({ rows: CARDS })
      .mockResolvedValueOnce({ rows: [{ id: 'new-tx' }] })
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).post('/api/transactions/import').send({
      period_id: 'p1',
      rows: [{ monto: '85000', tipo: 'expense', fecha: '2026-06-20', categoria: 'Alimentación', metodo: 'Crédito', tarjeta: 'visa gold' }],
    })

    expect(res.status).toBe(200)
    expect(res.body.created).toBe(1)
    const insertCall = query.mock.calls[4]
    expect(insertCall[1]).toContain('card1')
  })
})
