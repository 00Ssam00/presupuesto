import { Request, Response, NextFunction } from 'express'
import pool from '../db'

export async function listCategories(_req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const result = await pool.query(
      'SELECT * FROM categories WHERE is_active = TRUE ORDER BY sort_order, name'
    )
    res.json(result.rows)
  } catch (err) { next(err) }
}

export async function createCategory(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { name, color, icon, sort_order } = req.body
    const result = await pool.query(
      'INSERT INTO categories (name, color, icon, sort_order) VALUES ($1,$2,$3,$4) RETURNING *',
      [name, color || '#01696f', icon || null, sort_order || 0]
    )
    res.status(201).json(result.rows[0])
  } catch (err) { next(err) }
}

export async function updateCategory(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { name, color, icon, is_active, sort_order } = req.body
    const result = await pool.query(
      `UPDATE categories SET name=$1, color=$2, icon=$3, is_active=$4, sort_order=$5
       WHERE id=$6 RETURNING *`,
      [name, color, icon, is_active, sort_order, req.params.id]
    )
    if (result.rows.length === 0) { res.status(404).json({ error: 'No encontrada' }); return }
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

export async function deleteCategory(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    await pool.query('UPDATE categories SET is_active = FALSE WHERE id = $1', [req.params.id])
    res.status(204).send()
  } catch (err) { next(err) }
}
