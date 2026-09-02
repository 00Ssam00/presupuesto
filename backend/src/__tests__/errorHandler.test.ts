import { describe, it, expect, vi, afterEach } from 'vitest'
import { errorHandler } from '../middleware/errorHandler'

function mockRes() {
  const res: { statusCode?: number; body?: unknown; status: (c: number) => typeof res; json: (b: unknown) => void } = {
    status(code: number) {
      this.statusCode = code
      return this
    },
    json(body: unknown) {
      this.body = body
    },
  }
  return res
}

describe('errorHandler', () => {
  it('responde 500 con el mensaje del error', () => {
    const res = mockRes()
    const err = new Error('algo falló')
    vi.spyOn(console, 'error').mockImplementation(() => {})

    errorHandler(err, {} as never, res as never, vi.fn())

    expect(res.statusCode).toBe(500)
    expect(res.body).toEqual({ error: 'algo falló' })
  })

  it('usa un mensaje por defecto si el error no tiene message', () => {
    const res = mockRes()
    const err = new Error()
    vi.spyOn(console, 'error').mockImplementation(() => {})

    errorHandler(err, {} as never, res as never, vi.fn())

    expect(res.body).toEqual({ error: 'Error interno del servidor' })
  })

  describe('en producción', () => {
    const originalEnv = process.env.NODE_ENV

    afterEach(() => {
      process.env.NODE_ENV = originalEnv
    })

    it('no expone el mensaje crudo del error (puede filtrar detalles de Postgres)', () => {
      process.env.NODE_ENV = 'production'
      const res = mockRes()
      const err = new Error('relation "budgets" violates foreign key constraint "budgets_category_id_fkey"')
      vi.spyOn(console, 'error').mockImplementation(() => {})

      errorHandler(err, {} as never, res as never, vi.fn())

      expect(res.statusCode).toBe(500)
      expect(res.body).toEqual({ error: 'Error interno del servidor' })
    })

    it('igual registra el detalle completo en los logs del servidor', () => {
      process.env.NODE_ENV = 'production'
      const res = mockRes()
      const err = new Error('detalle sensible')
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

      errorHandler(err, {} as never, res as never, vi.fn())

      expect(consoleSpy).toHaveBeenCalledWith(err.stack)
    })
  })
})
