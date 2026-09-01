import { Request, Response, NextFunction } from 'express'
import pool from '../db'

export async function listPeriods(_req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const result = await pool.query('SELECT * FROM periods ORDER BY start_date DESC')
    res.json(result.rows)
  } catch (err) { next(err) }
}

export async function getCurrentPeriod(_req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const today = new Date().toISOString().split('T')[0]
    const result = await pool.query(
      'SELECT * FROM periods WHERE start_date <= $1 AND end_date >= $1 LIMIT 1',
      [today]
    )
    if (result.rows.length === 0) {
      const fallback = await pool.query(
        'SELECT * FROM periods ORDER BY start_date DESC LIMIT 1'
      )
      res.json(fallback.rows[0] || null)
      return
    }
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

export async function getPeriod(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const result = await pool.query('SELECT * FROM periods WHERE id = $1', [req.params.id])
    if (result.rows.length === 0) { res.status(404).json({ error: 'No encontrado' }); return }
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

export async function createPeriod(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { label, start_date, end_date, base_income, rollover } = req.body
    const result = await pool.query(
      `INSERT INTO periods (label, start_date, end_date, base_income, rollover)
       VALUES ($1,$2,$3,$4,$5) RETURNING *`,
      [label, start_date, end_date, base_income || 0, rollover || 0]
    )
    res.status(201).json(result.rows[0])
  } catch (err) { next(err) }
}

export async function updatePeriod(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { label, start_date, end_date, base_income, rollover, is_closed } = req.body
    const result = await pool.query(
      `UPDATE periods SET label=$1, start_date=$2, end_date=$3,
       base_income=$4, rollover=$5, is_closed=$6
       WHERE id=$7 RETURNING *`,
      [label, start_date, end_date, base_income, rollover, is_closed, req.params.id]
    )
    if (result.rows.length === 0) { res.status(404).json({ error: 'No encontrado' }); return }
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

export async function deletePeriod(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    await pool.query('DELETE FROM periods WHERE id = $1', [req.params.id])
    res.status(204).send()
  } catch (err) { next(err) }
}
