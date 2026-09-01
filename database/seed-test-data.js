// Genera datos de prueba sintéticos para desarrollo/testing.
// Reemplaza TODOS los datos transaccionales (periods, transactions, budgets,
// goals, upcoming_payments) y el catálogo de categorías/tarjetas por un set
// limpio y actualizado que cubre las 24 quincenas de 2026.
//
// Uso: node database/seed-test-data.js
//
// Es destructivo: trunca las tablas listadas abajo antes de volver a poblarlas.
// No toca `payment_methods` salvo marcar `requires_card = TRUE` en "Débito"
// (ahora que existe una tarjeta débito para asociar).

const path = require('path')
const { Pool } = require('pg')
require('dotenv').config({ path: path.resolve(__dirname, '../.env') })

const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false },
})

const KEEP_CATEGORIES = ['Alimentación', 'Transporte', 'Tecnología', 'Ahorro', 'Deudas & Pagos']

const CARDS = [
  { name: 'Visa Gold',        bank: 'Bancolombia', last_four: '4821', kind: 'credit' },
  { name: 'Mastercard Black', bank: 'Davivienda',   last_four: '9013', kind: 'credit' },
  { name: 'Débito Nu',        bank: 'Nu',           last_four: '2277', kind: 'debit' },
]

const EXPENSE_DESCRIPTIONS = {
  'Alimentación':   ['Mercado semanal', 'Almuerzo', 'Café', 'Domicilio comida', 'Panadería', 'Restaurante'],
  'Transporte':     ['Gasolina', 'Pasajes bus', 'Aplicación transporte', 'Parqueadero', 'Peaje'],
  'Tecnología':     ['Suscripción streaming', 'Datos móvil', 'Accesorio USB', 'App de pago'],
  'Ahorro':         ['Aporte fondo emergencia', 'Traslado cuenta ahorro'],
  'Deudas & Pagos':  ['Pago tarjeta de crédito', 'Cuota préstamo', 'Servicio público'],
}

function pad(n) { return String(n).padStart(2, '0') }
function iso(y, m, d) { return `${y}-${pad(m)}-${pad(d)}` }
function lastDayOfMonth(y, m) { return new Date(y, m, 0).getDate() }
function randInt(min, max) { return Math.floor(Math.random() * (max - min + 1)) + min }
function pick(arr) { return arr[randInt(0, arr.length - 1)] }

const MONTHS = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre',
]

function buildPeriods(year) {
  const periods = []
  for (let m = 1; m <= 12; m++) {
    const last = lastDayOfMonth(year, m)
    periods.push({ label: `1ra Quincena ${MONTHS[m - 1]}`, start: iso(year, m, 1), end: iso(year, m, 15) })
    periods.push({ label: `2da Quincena ${MONTHS[m - 1]}`, start: iso(year, m, 16), end: iso(year, m, last) })
  }
  return periods
}

async function main() {
  const client = await pool.connect()
  try {
    await client.query('BEGIN')

    console.log('Truncando datos transaccionales...')
    await client.query('TRUNCATE upcoming_payments, budgets, transactions, goals, periods, cards RESTART IDENTITY CASCADE')

    console.log('Depurando categorías...')
    await client.query('DELETE FROM categories WHERE name != ALL($1)', [KEEP_CATEGORIES])
    for (let i = 0; i < KEEP_CATEGORIES.length; i++) {
      await client.query('UPDATE categories SET sort_order = $1 WHERE name = $2', [i + 1, KEEP_CATEGORIES[i]])
    }
    const { rows: categories } = await client.query('SELECT id, name FROM categories ORDER BY sort_order')
    const catByName = Object.fromEntries(categories.map(c => [c.name, c.id]))

    console.log('Creando tarjetas...')
    const cardIds = {}
    for (const c of CARDS) {
      const { rows } = await client.query(
        'INSERT INTO cards (name, last_four, bank) VALUES ($1,$2,$3) RETURNING id',
        [c.name, c.last_four, c.bank]
      )
      cardIds[c.kind] = cardIds[c.kind] || []
      cardIds[c.kind].push(rows[0].id)
    }

    console.log('Ajustando métodos de pago...')
    await client.query(`UPDATE payment_methods SET requires_card = TRUE WHERE name = 'Débito'`)
    const { rows: methods } = await client.query('SELECT id, name, requires_card FROM payment_methods')
    const methodByName = Object.fromEntries(methods.map(m => [m.name, m]))
    const CASH = methodByName['Efectivo']
    const CREDIT = methodByName['Tarjeta Crédito']
    const DEBIT = methodByName['Débito']
    const TRANSFER = methodByName['Nequi / Transferencia']

    const today = new Date()
    const todayIso = iso(today.getFullYear(), today.getMonth() + 1, today.getDate())
    const periods = buildPeriods(2026)

    console.log('Generando quincenas, presupuestos y transacciones...')
    let rollover = 0
    let currentPeriodId = null

    for (const p of periods) {
      const isPast = p.end < todayIso
      const isCurrent = p.start <= todayIso && todayIso <= p.end
      const baseIncome = randInt(1100000, 1300000)

      const { rows: [period] } = await client.query(
        `INSERT INTO periods (label, start_date, end_date, base_income, rollover, is_closed)
         VALUES ($1,$2,$3,$4,$5,$6) RETURNING id`,
        [p.label, p.start, p.end, baseIncome, rollover, isPast]
      )
      if (isCurrent) currentPeriodId = period.id

      // Presupuesto: reparte el ingreso disponible entre las 5 categorías con pesos fijos.
      const available = baseIncome + rollover
      const weights = { 'Alimentación': 0.28, 'Transporte': 0.16, 'Tecnología': 0.12, 'Ahorro': 0.24, 'Deudas & Pagos': 0.20 }
      for (const name of KEEP_CATEGORIES) {
        const amount = Math.max(0, Math.round(available * weights[name]))
        await client.query(
          `INSERT INTO budgets (period_id, category_id, amount, percentage) VALUES ($1,$2,$3,$4)`,
          [period.id, catByName[name], amount, Math.round(weights[name] * 10000) / 100]
        )
      }

      let totalExpenses = 0
      let totalIncomeExtra = 0

      // El gasto total del período apunta a una fracción del disponible (entre 70% y
      // 115%, permitiendo déficit ocasional) para que el rollover fluctúe de forma
      // realista en vez de crecer sin límite quincena tras quincena. Se calcula siempre
      // (incluso para quincenas futuras, como gasto proyectado) para que el rollover
      // se mantenga estable; solo se insertan transacciones reales para quincenas ya
      // transcurridas o en curso (regla UX: el dashboard no muestra quincenas futuras).
      const spendRatio = 0.7 + Math.random() * 0.45
      const targetSpend = Math.max(0, Math.round(available * spendRatio))

      if (isPast || isCurrent) {
        const numTx = randInt(6, 12)
        const [y, m] = p.start.split('-').map(Number)
        const dayMax = Number(p.end.split('-')[2])
        const dayMin = Number(p.start.split('-')[2])
        const rawWeights = Array.from({ length: numTx }, () => Math.random())
        const weightSum = rawWeights.reduce((a, b) => a + b, 0)

        for (let i = 0; i < numTx; i++) {
          const catName = pick(KEEP_CATEGORIES)
          const desc = pick(EXPENSE_DESCRIPTIONS[catName])
          const share = weightSum > 0 ? rawWeights[i] / weightSum : 1 / numTx
          const amount = Math.max(2000, Math.round((targetSpend * share) / 500) * 500)
          const day = randInt(dayMin, Math.min(dayMax, isCurrent ? Number(todayIso.split('-')[2]) : dayMax))

          let method = pick([CASH, DEBIT, CREDIT, TRANSFER])
          let cardId = null
          if (method.id === CREDIT.id) cardId = pick(cardIds.credit)
          if (method.id === DEBIT.id) cardId = cardIds.debit[0]

          const date = iso(y, m, Math.max(day, dayMin))
          await client.query(
            `INSERT INTO transactions (period_id, category_id, payment_method_id, card_id, type, amount, description, date)
             VALUES ($1,$2,$3,$4,'expense',$5,$6,$7)`,
            [period.id, catByName[catName], method.id, cardId, amount, desc, date]
          )
          totalExpenses += amount
        }

        // Un ingreso extra ocasional (ej. reembolso, ingreso adicional).
        if (Math.random() < 0.3) {
          const extra = randInt(20000, 80000)
          await client.query(
            `INSERT INTO transactions (period_id, category_id, payment_method_id, type, amount, description, date)
             VALUES ($1,$2,$3,'income',$4,'Ingreso adicional',$5)`,
            [period.id, catByName['Ahorro'], TRANSFER.id, extra, p.start]
          )
          totalIncomeExtra += extra
        }
      }

      // Para quincenas futuras no hay transacciones reales (totalExpenses = 0): se usa el
      // gasto proyectado (targetSpend) para que el rollover siga fluctuando de forma
      // realista en vez de acumular el ingreso completo sin gastar.
      const expensesForRollover = (isPast || isCurrent) ? totalExpenses : targetSpend
      rollover = Math.round((available + totalIncomeExtra - expensesForRollover) * 100) / 100
    }

    console.log('Creando metas de ejemplo...')
    await client.query(
      `INSERT INTO goals (name, type, target_amount, current_amount, deadline) VALUES
        ('Fondo de emergencia', 'savings', 5000000, 1200000, '2026-12-31'),
        ('Portafolio inversión', 'investment', 3000000, 450000, NULL)`
    )

    console.log('Creando próximos pagos de ejemplo...')
    if (currentPeriodId) {
      const nextMonthDay = todayIso
      await client.query(
        `INSERT INTO upcoming_payments (period_id, category_id, name, amount, due_date, status, is_recurring, recurrence_interval) VALUES
          ($1,$2,'Suscripción streaming', 25000, $3, 'pending', TRUE, 'monthly'),
          ($1,$4,'Pago tarjeta de crédito', 180000, $3, 'pending', TRUE, 'monthly')`,
        [currentPeriodId, catByName['Tecnología'], nextMonthDay, catByName['Deudas & Pagos']]
      )
    }

    await client.query('COMMIT')
    console.log('Listo. Datos de prueba generados para 2026.')
  } catch (err) {
    await client.query('ROLLBACK')
    console.error('Error generando datos, se revirtió todo:', err)
    process.exitCode = 1
  } finally {
    client.release()
    await pool.end()
  }
}

main()
