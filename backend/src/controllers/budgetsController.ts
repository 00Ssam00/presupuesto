import { Request, Response, NextFunction } from 'express'
import pool from '../db'

export async function getBudgetsByPeriod(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const result = await pool.query(
      `SELECT b.id, $1::uuid AS period_id, c.id AS category_id,
        COALESCE(b.amount, 0) AS amount, b.percentage,
        c.name AS category_name, c.color AS category_color,
        COALESCE(
          (SELECT SUM(amount) FROM transactions t
           WHERE t.category_id = c.id
             AND t.period_id   = $1
             AND t.type        = 'expense'), 0
        ) AS spent
       FROM categories c
       LEFT JOIN budgets b ON b.period_id = $1 AND b.category_id = c.id
       ORDER BY c.sort_order`,
      [req.params.periodId]
    )
    res.json(result.rows)
  } catch (err) { next(err) }
}

export async function upsertBudget(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { period_id, category_id, amount, percentage } = req.body
    const result = await pool.query(
      `INSERT INTO budgets (period_id, category_id, amount, percentage)
       VALUES ($1,$2,$3,$4)
       ON CONFLICT (period_id, category_id)
       DO UPDATE SET amount=$3, percentage=$4
       RETURNING *`,
      [period_id, category_id, amount, percentage || null]
    )
    res.status(201).json(result.rows[0])
  } catch (err) { next(err) }
}

export async function deleteBudget(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    await pool.query('DELETE FROM budgets WHERE id = $1', [req.params.id])
    res.status(204).send()
  } catch (err) { next(err) }
}
