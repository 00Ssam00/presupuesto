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
    // El formulario de edición solo envía name/color — icon, is_active y
    // sort_order no vienen en el body. Sin COALESCE, esos `undefined` se
    // volvían NULL en la fila (is_active=NULL hacía que la categoría
    // desapareciera de todos los listados, que filtran is_active = TRUE).
    const result = await pool.query(
      `UPDATE categories SET name=$1, color=$2,
         icon=COALESCE($3, icon), is_active=COALESCE($4, is_active), sort_order=COALESCE($5, sort_order)
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
