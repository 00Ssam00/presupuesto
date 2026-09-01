import { Request, Response, NextFunction } from 'express'
import pool from '../db'

export async function getDashboard(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { periodId } = req.params

    const periodRes = await pool.query(
      'SELECT * FROM periods WHERE id = $1',
      [periodId]
    )
    if (periodRes.rows.length === 0) {
      res.status(404).json({ error: 'Período no encontrado' })
      return
    }
    const period = periodRes.rows[0]

    const totalsRes = await pool.query(
      `SELECT
        COALESCE(SUM(CASE WHEN type = 'expense' THEN amount ELSE 0 END), 0) AS total_expenses,
        COALESCE(SUM(CASE WHEN type = 'income'  THEN amount ELSE 0 END), 0) AS total_income_extra
       FROM transactions WHERE period_id = $1`,
      [periodId]
    )
    const { total_expenses, total_income_extra } = totalsRes.rows[0]

    const available = Number(period.base_income) + Number(period.rollover) + Number(total_income_extra)
    const remaining = available - Number(total_expenses)

    const byCategoryRes = await pool.query(
      `SELECT
        c.id   AS category_id,
        c.name AS category_name,
        c.color AS category_color,
        COALESCE(SUM(t.amount), 0) AS total,
        COALESCE(b.amount, 0)      AS budget
       FROM categories c
       LEFT JOIN transactions t
         ON t.category_id = c.id AND t.period_id = $1 AND t.type = 'expense'
       LEFT JOIN budgets b
         ON b.category_id = c.id AND b.period_id = $1
       WHERE c.is_active = TRUE
       GROUP BY c.id, c.name, c.color, b.amount
       ORDER BY total DESC`,
      [periodId]
    )

    const upcomingRes = await pool.query(
      `SELECT up.*, c.name AS category_name, c.color AS category_color
       FROM upcoming_payments up
       JOIN categories c ON c.id = up.category_id
       WHERE (up.period_id = $1 OR up.period_id IS NULL)
         AND up.status = 'pending'
       ORDER BY up.due_date ASC
       LIMIT 5`,
      [periodId]
    )

    res.json({
      period,
      available,
      total_expenses: Number(total_expenses),
      total_income_extra: Number(total_income_extra),
      remaining,
      expenses_by_category: byCategoryRes.rows,
      upcoming_payments: upcomingRes.rows,
    })
  } catch (err) {
    next(err)
  }
}
