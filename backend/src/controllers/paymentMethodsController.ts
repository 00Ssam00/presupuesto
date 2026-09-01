import { Request, Response, NextFunction } from 'express'
import pool from '../db'

export async function listPaymentMethods(_req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const result = await pool.query(
      'SELECT * FROM payment_methods WHERE is_active = TRUE ORDER BY name'
    )
    res.json(result.rows)
  } catch (err) { next(err) }
}
