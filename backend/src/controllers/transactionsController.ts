import { Request, Response, NextFunction } from 'express'
import pool from '../db'
import { recalculateAllRollovers } from '../services/rollover'

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
    await recalculateAllRollovers()
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
    await recalculateAllRollovers()
    res.json(result.rows[0])
  } catch (err) { next(err) }
}

interface ImportRow {
  monto?: string
  tipo?: string
  fecha?: string
  categoria?: string
  metodo?: string
  tarjeta?: string
  descripcion?: string
}

interface ImportRowResult {
  row: number
  status: 'created' | 'error'
  id?: string
  error?: string
}

function findByName<T extends { name: string }>(list: T[], name: string | undefined): T | undefined {
  const needle = (name || '').trim().toLowerCase()
  return list.find(x => x.name.toLowerCase() === needle)
}

// Importación masiva de transacciones desde el CSV de Importar/Exportar.
// Todas las filas se crean en la misma quincena (period_id) elegida en la UI —
// evita inferir la quincena por fecha fila por fila, que sería más frágil y
// menos consistente con la regla del resto de la app (la fecha de una
// transacción debe caer dentro del rango de su propia quincena).
// La importación es parcial: cada fila se valida y reporta por separado, así
// una fila con datos inválidos no bloquea el resto del archivo.
export async function importTransactions(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { period_id, rows } = req.body as { period_id?: string; rows?: ImportRow[] }
    if (!period_id || !Array.isArray(rows) || rows.length === 0) {
      res.status(400).json({ error: 'Se requiere period_id y al menos una fila' })
      return
    }

    const periodResult = await pool.query('SELECT * FROM periods WHERE id = $1', [period_id])
    if (periodResult.rows.length === 0) { res.status(404).json({ error: 'Quincena no encontrada' }); return }
    const period = periodResult.rows[0]

    const [catsRes, methodsRes, cardsRes] = await Promise.all([
      pool.query('SELECT id, name FROM categories WHERE is_active = TRUE'),
      pool.query('SELECT id, name, requires_card FROM payment_methods WHERE is_active = TRUE'),
      pool.query('SELECT id, name FROM cards WHERE is_active = TRUE'),
    ])

    const errors: ImportRowResult[] = []
    const toInsert: {
      row: number; category_id: string; payment_method_id: string; card_id: string | null
      type: string; amount: number; description: string; date: string
    }[] = []

    rows.forEach((row, i) => {
      const rowNum = i + 1
      const amount = parseFloat(row.monto || '')
      if (!row.monto || Number.isNaN(amount) || amount <= 0) {
        errors.push({ row: rowNum, status: 'error', error: `Monto inválido: "${row.monto || ''}"` })
        return
      }
      const type = (row.tipo || '').trim().toLowerCase()
      if (type !== 'income' && type !== 'expense') {
        errors.push({ row: rowNum, status: 'error', error: `Tipo debe ser "income" o "expense", no "${row.tipo || ''}"` })
        return
      }
      const category = findByName(catsRes.rows, row.categoria)
      if (!category) {
        errors.push({ row: rowNum, status: 'error', error: `Categoría no encontrada: "${row.categoria || ''}"` })
        return
      }
      const method = findByName(methodsRes.rows, row.metodo)
      if (!method) {
        errors.push({ row: rowNum, status: 'error', error: `Método de pago no encontrado: "${row.metodo || ''}"` })
        return
      }
      let card_id: string | null = null
      if (method.requires_card) {
        const card = findByName(cardsRes.rows, row.tarjeta)
        if (!card) {
          errors.push({ row: rowNum, status: 'error', error: `El método "${method.name}" requiere tarjeta: "${row.tarjeta || ''}" no encontrada` })
          return
        }
        card_id = card.id
      }
      const date = (row.fecha || '').trim() || period.start_date
      if (!/^\d{4}-\d{2}-\d{2}$/.test(date)) {
        errors.push({ row: rowNum, status: 'error', error: `Fecha inválida: "${date}" (usar YYYY-MM-DD)` })
        return
      }
      if (date < period.start_date || date > period.end_date) {
        errors.push({ row: rowNum, status: 'error', error: `La fecha ${date} está fuera del rango de la quincena (${period.start_date} — ${period.end_date})` })
        return
      }
      const description = (row.descripcion || '').trim() || 'Importado'

      toInsert.push({ row: rowNum, category_id: category.id, payment_method_id: method.id, card_id, type, amount, description, date })
    })

    const created = await Promise.all(toInsert.map(t =>
      pool.query(
        `INSERT INTO transactions (period_id, category_id, payment_method_id, card_id, type, amount, description, date)
         VALUES ($1,$2,$3,$4,$5,$6,$7,$8) RETURNING id`,
        [period_id, t.category_id, t.payment_method_id, t.card_id, t.type, t.amount, t.description, t.date]
      ).then((r): ImportRowResult => ({ row: t.row, status: 'created', id: r.rows[0].id }))
    ))

    if (created.length > 0) await recalculateAllRollovers()

    const results = [...errors, ...created].sort((a, b) => a.row - b.row)
    res.status(200).json({ created: created.length, failed: errors.length, results })
  } catch (err) { next(err) }
}

export async function deleteTransaction(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    await pool.query('DELETE FROM transactions WHERE id = $1', [req.params.id])
    await recalculateAllRollovers()
    res.status(204).send()
  } catch (err) { next(err) }
}
