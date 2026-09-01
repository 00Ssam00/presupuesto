import { Request, Response, NextFunction } from 'express'
import pool from '../db'

const SELECT_WITH_JOINS = `
  SELECT t.*,
    c.name  AS category_name,
    c.color AS category_color,
    pm.name AS payment_method_name,
    ca.name AS card_name
  FROM transactions t
  JOIN categories     c  ON c.id  = t.category_id
  JOIN payment_methods pm ON pm.id = t.payment_method_id
  LEFT JOIN cards     ca ON ca.id = t.card_id
`

export async function listTransactions(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { period_id, type, category_id } = req.query
    const conditions: string[] = []
    const params: unknown[] = []

    if (period_id)   { params.push(period_id);   conditions.push(`t.period_id = $${params.length}`) }
    if (type)        { params.push(type);         conditions.push(`t.type = $${params.length}`) }
    if (category_id) { params.push(category_id);  conditions.push(`t.category_id = $${params.length}`) }

    const where = conditions.length ? `WHERE ${conditions.join(' AND ')}` : ''
    const result = await pool.query(
      `${SELECT_WITH_JOINS} ${where} ORDER BY t.date DESC, t.created_at DESC`,
      params
    )
    res.json(result.rows)
  } catch (err) { next(err) }
}

export async function getTransaction(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const result = await pool.query(`${SELECT_WITH_JOINS} WHERE t.id = $1`, [req.params.id])
    if (result.rows.length === 0) { res.status(404).json({ error: 'No encontrada' }); return }
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

export async function createTransaction(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { period_id, category_id, payment_method_id, card_id, type, amount, description, note, date } = req.body
    const result = await pool.query(
      `INSERT INTO transactions
         (period_id, category_id, payment_method_id, card_id, type, amount, description, note, date)
       VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9) RETURNING *`,
      [period_id, category_id, payment_method_id, card_id || null, type, amount, description, note || null, date]
    )
    res.status(201).json(result.rows[0])
  } catch (err) { next(err) }
}

export async function updateTransaction(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { category_id, payment_method_id, card_id, type, amount, description, note, date } = req.body
    const result = await pool.query(
      `UPDATE transactions
       SET category_id=$1, payment_method_id=$2, card_id=$3, type=$4,
           amount=$5, description=$6, note=$7, date=$8, updated_at=NOW()
       WHERE id=$9 RETURNING *`,
      [category_id, payment_method_id, card_id || null, type, amount, description, note || null, date, req.params.id]
    )
    if (result.rows.length === 0) { res.status(404).json({ error: 'No encontrada' }); return }
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

export async function deleteTransaction(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    await pool.query('DELETE FROM transactions WHERE id = $1', [req.params.id])
    res.status(204).send()
  } catch (err) { next(err) }
}
