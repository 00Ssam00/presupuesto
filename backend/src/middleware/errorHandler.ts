import { Request, Response, NextFunction } from 'express'

const GENERIC_MESSAGE = 'Error interno del servidor'

export function errorHandler(
  err: Error,
  _req: Request,
  res: Response,
  _next: NextFunction
): void {
  console.error(err.stack)
  // En producción no se expone err.message crudo (puede filtrar detalles de
  // Postgres, como nombres de tablas/columnas) — solo un mensaje genérico.
  // El detalle completo sigue disponible en los logs del servidor de arriba.
  const message = process.env.NODE_ENV === 'production'
    ? GENERIC_MESSAGE
    : (err.message || GENERIC_MESSAGE)
  res.status(500).json({ error: message })
}
