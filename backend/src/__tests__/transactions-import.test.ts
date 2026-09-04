import { describe, it, expect, vi, beforeEach } from 'vitest'
import request from 'supertest'

vi.mock('../db', () => ({
  default: { query: vi.fn() },
}))

import pool from '../db'
import app from '../app'

const query = (pool as unknown as { query: ReturnType<typeof vi.fn> }).query

const PERIOD_A = { id: 'pA', start_date: '2026-06-01', end_date: '2026-06-15' }
const PERIOD_B = { id: 'pB', start_date: '2026-06-16', end_date: '2026-06-30' }
const CATEGORIES = [{ id: 'c1', name: 'Alimentación' }]
const METHODS_NO_CARD = [{ id: 'pm1', name: 'Efectivo', requires_card: false }]
const METHODS_CARD = [{ id: 'pm2', name: 'Crédito', requires_card: true }]
const CARDS = [{ id: 'card1', name: 'Visa Gold' }]

beforeEach(() => {
  query.mockReset()
})

describe('POST /api/transactions/import', () => {
  it('rechaza sin rows', async () => {
    const res = await request(app).post('/api/transactions/import').send({})
    expect(res.status).toBe(400)
    expect(query).not.toHaveBeenCalled()
  })

  it('rechaza un array de rows vacío', async () => {
    const res = await request(app).post('/api/transactions/import').send({ rows: [] })
    expect(res.status).toBe(400)
  })

  it('importa una fila válida resolviendo la quincena por su fecha, y recalcula los rollovers', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD_A, PERIOD_B] }) // SELECT periods
      .mockResolvedValueOnce({ rows: CATEGORIES }) // SELECT categories
      .mockResolvedValueOnce({ rows: METHODS_NO_CARD }) // SELECT payment_methods
      .mockResolvedValueOnce({ rows: [] }) // SELECT cards
      .mockResolvedValueOnce({ rows: [{ id: 'new-tx' }] }) // INSERT
      .mockResolvedValueOnce({ rows: [] }) // SELECT de recalculateAllRollovers

    const res = await request(app).post('/api/transactions/import').send({
      rows: [{ monto: '85000', tipo: 'expense', fecha: '2026-06-20', categoria: 'Alimentación', metodo: 'Efectivo', descripcion: 'Mercado' }],
    })

    expect(res.status).toBe(200)
    expect(res.body).toEqual({
      created: 1, failed: 0,
      results: [{ row: 1, status: 'created', id: 'new-tx' }],
    })
    const insertCall = query.mock.calls[4]
    expect(insertCall[1][0]).toBe('pB') // period_id resuelto por la fecha (cae en PERIOD_B)
  })

  it('importa un archivo que abarca dos quincenas distintas en una sola pasada', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD_A, PERIOD_B] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_NO_CARD })
      .mockResolvedValueOnce({ rows: [] })
      .mockResolvedValueOnce({ rows: [{ id: 'tx-a' }] })
      .mockResolvedValueOnce({ rows: [{ id: 'tx-b' }] })
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).post('/api/transactions/import').send({
      rows: [
        { monto: '10000', tipo: 'expense', fecha: '2026-06-05', categoria: 'Alimentación', metodo: 'Efectivo' },
        { monto: '20000', tipo: 'expense', fecha: '2026-06-20', categoria: 'Alimentación', metodo: 'Efectivo' },
      ],
    })

    expect(res.status).toBe(200)
    expect(res.body.created).toBe(2)
    const insertA = query.mock.calls[4]
    const insertB = query.mock.calls[5]
    expect(insertA[1][0]).toBe('pA')
    expect(insertB[1][0]).toBe('pB')
  })

  it('rechaza filas sin fecha o con fecha inválida (ahora es obligatoria)', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD_A, PERIOD_B] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_NO_CARD })
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).post('/api/transactions/import').send({
      rows: [{ monto: '85000', tipo: 'expense', categoria: 'Alimentación', metodo: 'Efectivo' }],
    })

    expect(res.status).toBe(200)
    expect(res.body.created).toBe(0)
    expect(res.body.results[0].error).toContain('Fecha requerida')
    expect(query).toHaveBeenCalledTimes(4) // no INSERT ni recalculateAllRollovers
  })

  it('reporta un error claro cuando la fecha no cae en ninguna quincena existente', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD_A, PERIOD_B] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_NO_CARD })
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).post('/api/transactions/import').send({
      rows: [{ monto: '85000', tipo: 'expense', fecha: '2099-01-01', categoria: 'Alimentación', metodo: 'Efectivo' }],
    })

    expect(res.status).toBe(200)
    expect(res.body.failed).toBe(1)
    expect(res.body.results[0].error).toContain('ninguna quincena que incluya la fecha')
  })

  it('reporta por fila las categorías/métodos inexistentes sin bloquear el resto (importación parcial)', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD_A, PERIOD_B] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_NO_CARD })
      .mockResolvedValueOnce({ rows: [] })
      .mockResolvedValueOnce({ rows: [{ id: 'new-tx' }] }) // solo la fila válida se inserta
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).post('/api/transactions/import').send({
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

  it('exige tarjeta cuando el método de pago la requiere', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD_A, PERIOD_B] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_CARD })
      .mockResolvedValueOnce({ rows: [] }) // sin tarjetas activas

    const res = await request(app).post('/api/transactions/import').send({
      rows: [{ monto: '85000', tipo: 'expense', fecha: '2026-06-20', categoria: 'Alimentación', metodo: 'Crédito', tarjeta: 'Visa Gold' }],
    })

    expect(res.status).toBe(200)
    expect(res.body.failed).toBe(1)
    expect(res.body.results[0].error).toContain('requiere tarjeta')
  })

  it('resuelve la tarjeta correctamente cuando existe', async () => {
    query
      .mockResolvedValueOnce({ rows: [PERIOD_A, PERIOD_B] })
      .mockResolvedValueOnce({ rows: CATEGORIES })
      .mockResolvedValueOnce({ rows: METHODS_CARD })
      .mockResolvedValueOnce({ rows: CARDS })
      .mockResolvedValueOnce({ rows: [{ id: 'new-tx' }] })
      .mockResolvedValueOnce({ rows: [] })

    const res = await request(app).post('/api/transactions/import').send({
      rows: [{ monto: '85000', tipo: 'expense', fecha: '2026-06-20', categoria: 'Alimentación', metodo: 'Crédito', tarjeta: 'visa gold' }],
    })

    expect(res.status).toBe(200)
    expect(res.body.created).toBe(1)
    const insertCall = query.mock.calls[4]
    expect(insertCall[1]).toContain('card1')
  })
})
