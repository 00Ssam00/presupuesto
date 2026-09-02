import pool from '../db'

interface PeriodAggRow {
  id: string
  start_date: string
  base_income: string | number
  rollover: string | number
  total_expenses: string | number
  total_income_extra: string | number
}

// Recalcula el rollover de todas las quincenas en cascada cronológica: el
// rollover de una quincena es el sobrante/déficit (base_income + rollover +
// ingresos extra - gastos) de la quincena inmediatamente anterior. La
// primera quincena cronológica siempre parte de rollover = 0.
export async function recalculateAllRollovers(): Promise<void> {
  const { rows } = await pool.query<PeriodAggRow>(`
    SELECT p.id, p.start_date, p.base_income, p.rollover,
      COALESCE(SUM(CASE WHEN t.type = 'expense' THEN t.amount ELSE 0 END), 0) AS total_expenses,
      COALESCE(SUM(CASE WHEN t.type = 'income'  THEN t.amount ELSE 0 END), 0) AS total_income_extra
    FROM periods p
    LEFT JOIN transactions t ON t.period_id = p.id
    GROUP BY p.id
    ORDER BY p.start_date ASC
  `)

  const updates: { id: string; rollover: number }[] = []
  let carry = 0
  for (let i = 0; i < rows.length; i++) {
    const p = rows[i]
    const expected = i === 0 ? 0 : Math.round(carry * 100) / 100
    if (Number(p.rollover) !== expected) {
      updates.push({ id: p.id, rollover: expected })
    }
    const available = Number(p.base_income) + expected + Number(p.total_income_extra)
    carry = available - Number(p.total_expenses)
  }

  if (updates.length === 0) return
  await Promise.all(
    updates.map(u => pool.query('UPDATE periods SET rollover = $1 WHERE id = $2', [u.rollover, u.id]))
  )
}
