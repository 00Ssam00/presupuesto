import { Request, Response, NextFunction } from 'express'
import pool from '../db'

export async function listCards(_req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const result = await pool.query(
      'SELECT * FROM cards WHERE is_active = TRUE ORDER BY name'
    )
    res.json(result.rows)
  } catch (err) { next(err) }
}

export async function createCard(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { name, last_four, bank } = req.body
    const result = await pool.query(
      'INSERT INTO cards (name, last_four, bank) VALUES ($1,$2,$3) RETURNING *',
      [name, last_four || null, bank || null]
    )
    res.status(201).json(result.rows[0])
  } catch (err) { next(err) }
}

export async function updateCard(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { name, last_four, bank, is_active } = req.body
    const result = await pool.query(
      'UPDATE cards SET name=$1, last_four=$2, bank=$3, is_active=$4 WHERE id=$5 RETURNING *',
      [name, last_four || null, bank || null, is_active, req.params.id]
    )
    if (result.rows.length === 0) { res.status(404).json({ error: 'No encontrada' }); return }
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

export async function deleteCard(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    await pool.query('UPDATE cards SET is_active = FALSE WHERE id = $1', [req.params.id])
    res.status(204).send()
  } catch (err) { next(err) }
}
