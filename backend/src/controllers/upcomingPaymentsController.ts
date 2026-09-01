import { Request, Response, NextFunction } from 'express'
import pool from '../db'

export async function listUpcomingPayments(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { period_id, status } = req.query
    const conditions: string[] = []
    const params: unknown[] = []

    if (period_id) { params.push(period_id); conditions.push(`up.period_id = $${params.length}`) }
    if (status)    { params.push(status);    conditions.push(`up.status = $${params.length}`) }

    const where = conditions.length ? `WHERE ${conditions.join(' AND ')}` : ''
    const result = await pool.query(
      `SELECT up.*, c.name AS category_name, c.color AS category_color
       FROM upcoming_payments up
       JOIN categories c ON c.id = up.category_id
       ${where} ORDER BY up.due_date ASC`,
      params
    )
    res.json(result.rows)
  } catch (err) { next(err) }
}

export async function createUpcomingPayment(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { period_id, category_id, name, amount, due_date, is_recurring, recurrence_interval } = req.body
    const result = await pool.query(
      `INSERT INTO upcoming_payments
         (period_id, category_id, name, amount, due_date, is_recurring, recurrence_interval)
       VALUES ($1,$2,$3,$4,$5,$6,$7) RETURNING *`,
      [period_id || null, category_id, name, amount, due_date, is_recurring || false, recurrence_interval || null]
    )
    res.status(201).json(result.rows[0])
  } catch (err) { next(err) }
}

export async function updateUpcomingPayment(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { category_id, name, amount, due_date, status, is_recurring, recurrence_interval } = req.body
    const result = await pool.query(
      `UPDATE upcoming_payments
       SET category_id=$1, name=$2, amount=$3, due_date=$4,
           status=$5, is_recurring=$6, recurrence_interval=$7
       WHERE id=$8 RETURNING *`,
      [category_id, name, amount, due_date, status, is_recurring, recurrence_interval || null, req.params.id]
    )
    if (result.rows.length === 0) { res.status(404).json({ error: 'No encontrado' }); return }
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

export async function deleteUpcomingPayment(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    await pool.query('DELETE FROM upcoming_payments WHERE id = $1', [req.params.id])
    res.status(204).send()
  } catch (err) { next(err) }
}
