import { Request, Response, NextFunction } from 'express'
import pool from '../db'

export async function listGoals(_req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const result = await pool.query(
      'SELECT * FROM goals WHERE is_active = TRUE ORDER BY type, name'
    )
    res.json(result.rows)
  } catch (err) { next(err) }
}

export async function createGoal(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { name, type, target_amount, current_amount, deadline } = req.body
    const result = await pool.query(
      `INSERT INTO goals (name, type, target_amount, current_amount, deadline)
       VALUES ($1,$2,$3,$4,$5) RETURNING *`,
      [name, type, target_amount, current_amount || 0, deadline || null]
    )
    res.status(201).json(result.rows[0])
  } catch (err) { next(err) }
}

export async function updateGoal(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { name, type, target_amount, current_amount, deadline, is_active } = req.body
    // El formulario de edición no envía is_active — sin COALESCE, ese
    // `undefined` se volvía NULL y la meta desaparecía de los listados
    // (que filtran is_active = TRUE).
    const result = await pool.query(
      `UPDATE goals SET name=$1, type=$2, target_amount=$3,
       current_amount=$4, deadline=$5, is_active=COALESCE($6, is_active), updated_at=NOW()
       WHERE id=$7 RETURNING *`,
      [name, type, target_amount, current_amount, deadline || null, is_active, req.params.id]
    )
    if (result.rows.length === 0) { res.status(404).json({ error: 'No encontrada' }); return }
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

export async function deleteGoal(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    await pool.query('UPDATE goals SET is_active = FALSE WHERE id = $1', [req.params.id])
    res.status(204).send()
  } catch (err) { next(err) }
}
