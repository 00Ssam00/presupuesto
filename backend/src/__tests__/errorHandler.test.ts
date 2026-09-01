import { describe, it, expect, vi } from 'vitest'
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
})
