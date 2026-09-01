import { Request, Response, NextFunction } from 'express'
import pool from '../db'

export async function getSettings(_req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const result = await pool.query('SELECT key, value FROM app_settings')
    const settings: Record<string, string> = {}
    result.rows.forEach((r: { key: string; value: string }) => { settings[r.key] = r.value })
    res.json(settings)
  } catch (err) { next(err) }
}

export async function updateSetting(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { key } = req.params
    const { value } = req.body
    await pool.query(
      `INSERT INTO app_settings (key, value, updated_at)
       VALUES ($1,$2,NOW())
       ON CONFLICT (key) DO UPDATE SET value=$2, updated_at=NOW()`,
      [key, value]
    )
    res.json({ key, value })
  } catch (err) { next(err) }
}
