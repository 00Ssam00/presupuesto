import { useEffect, useState } from 'react'
import {
  PieChart, Pie, Cell, Tooltip, ResponsiveContainer,
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Legend
} from 'recharts'
import Badge from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import Modal from '../../components/ui/Modal'
import FormField from '../../components/ui/FormField'
import {
  getDashboard, getCurrentPeriod, getPeriods,
  getCategories, getPaymentMethods, getCards, createTransaction
} from '../../lib/api'
import { formatCOP, formatShortDate, pct, todayISO } from '../../lib/utils'
import type { DashboardData, Period, Category, PaymentMethod, Card } from '../../types'
import styles from './Dashboard.module.css'

const EMPTY_TX = {
  period_id: '', category_id: '', payment_method_id: '',
  card_id: '', type: 'expense' as const,
  amount: '', description: '', date: todayISO(),
}

export default function Dashboard() {
  const [data, setData]           = useState<DashboardData | null>(null)
  const [periods, setPeriods]     = useState<Period[]>([])
  const [selectedId, setSelectedId] = useState('')
  const [loading, setLoading]     = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError]         = useState('')

  const [categories, setCategories]   = useState<Category[]>([])
  const [methods, setMethods]         = useState<PaymentMethod[]>([])
  const [cards, setCards]             = useState<Card[]>([])
  const [txModal, setTxModal]         = useState(false)
  const [txForm, setTxForm]           = useState({ ...EMPTY_TX })
  const [saving, setSaving]           = useState(false)

  useEffect(() => {
    Promise.all([getCurrentPeriod(), getPeriods(), getCategories(), getPaymentMethods(), getCards()])
      .then(([current, all, cats, meths, cds]) => {
        setPeriods(all)
        setCategories(cats)
        setMethods(meths)
        setCards(cds)
        const id = current?.id || all[0]?.id || ''
        setSelectedId(id)
        if (!id) setLoading(false)
      })
      .catch(() => {
        setError('No se pudo conectar al servidor.')
        setLoading(false)
      })
  }, [])

  useEffect(() => {
    if (!selectedId) return
    setError('')
    if (!data) {
      setLoading(true)
    } else {
      setRefreshing(true)
    }
    getDashboard(selectedId)
      .then(setData)
      .catch(() => setError('Error al cargar el dashboard.'))
      .finally(() => { setLoading(false); setRefreshing(false) })
  }, [selectedId])

  function openTxModal() {
    setTxForm({ ...EMPTY_TX, period_id: selectedId })
    setTxModal(true)
  }

  async function handleAddTx(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    try {
      await createTransaction({
        ...txForm,
        amount: parseFloat(txForm.amount),
        card_id: requiresCard ? txForm.card_id : undefined,
      })
      setTxModal(false)
      getDashboard(selectedId).then(setData)
    } finally {
      setSaving(false)
    }
  }

  const requiresCard = methods.find(m => m.id === txForm.payment_method_id)?.requires_card
  const period = data?.period

  const periodLabel = (() => {
    if (!period) return ''
    const start = new Date(period.start_date)
    const end   = new Date(period.end_date)
    return `${start.getUTCDate()}–${end.getUTCDate()}`
  })()

  const sortedPeriods = [...periods].sort(
    (a, b) => new Date(a.start_date).getTime() - new Date(b.start_date).getTime()
  )
  const currentIdx = sortedPeriods.findIndex(p => p.id === selectedId)
  const goPrev = () => { if (currentIdx > 0) setSelectedId(sortedPeriods[currentIdx - 1].id) }
  const goNext = () => { if (currentIdx < sortedPeriods.length - 1) setSelectedId(sortedPeriods[currentIdx + 1].id) }

  // `total` llega como string desde Postgres (NUMERIC) — Recharts necesita un
  // number en dataKey para calcular los ángulos del pie, si no, no dibuja nada.
  const pieData = (data?.expenses_by_category || [])
    .map(c => ({ ...c, total: Number(c.total) }))
    .filter(c => c.total > 0)
  const barData = (data?.expenses_by_category || []).map(c => ({
    name: c.category_name.length > 8 ? c.category_name.slice(0, 8) + '…' : c.category_name,
    Presupuesto: Number(c.budget),
    Gasto: Number(c.total),
  }))

  const totalBudget   = (data?.expenses_by_category || []).reduce((s, c) => s + Number(c.budget), 0)
  const budgetRemain  = totalBudget - (data?.total_expenses || 0)

  if (error) return (
    <div className={styles.stateMsg} style={{ color: 'var(--color-warning)' }}>{error}</div>
  )
  if (loading) return <div className={styles.stateMsg}>Cargando...</div>
  if (!data)   return <div className={styles.stateMsg}>Sin datos. Ejecuta el seed en la base de datos.</div>

  const { available, total_expenses, remaining } = data

  return (
    <div className={refreshing ? styles.refreshing : ''}>
      {/* Header */}
      <div className={styles.pageHeader}>
        <div>
          <h1 className={styles.pageTitle}>Dashboard</h1>
          <p className={styles.pageSubtitle}>Saldo disponible, rollover, presupuesto y pagos próximos.</p>
        </div>
        <div className={styles.headerRight}>
          <div className={styles.periodControl}>
            <button className={styles.periodBtn} onClick={goPrev}
              disabled={currentIdx <= 0} aria-label="Quincena anterior">‹</button>
            <select
              className={styles.periodSelect}
              value={selectedId}
              onChange={e => setSelectedId(e.target.value)}
            >
              {sortedPeriods.map(p => (
                <option key={p.id} value={p.id}>{p.label}</option>
              ))}
            </select>
            {periodLabel && <span className={styles.periodDates}>{periodLabel}</span>}
            <button className={styles.periodBtn} onClick={goNext}
              disabled={currentIdx >= sortedPeriods.length - 1} aria-label="Quincena siguiente">›</button>
          </div>
          <Button onClick={openTxModal}>+ Agregar gasto</Button>
        </div>
      </div>

      {/* Hero — Saldo disponible */}
      <div className={styles.heroCard}>
        <div className={styles.heroLeft}>
          <div className={styles.heroLabel}>SALDO DISPONIBLE</div>
          <div className={styles.heroAmount}>{formatCOP(available)}</div>
          <div className={styles.heroSub}>Ingreso quincenal + rollover anterior.</div>
          <div className={styles.heroBreakdown}>
            <div className={styles.breakdownItem}>
              <span className={styles.breakdownLabel}>INGRESO</span>
              <span className={styles.breakdownValue}>{formatCOP(Number(period?.base_income))}</span>
            </div>
            <div className={styles.breakdownDivider} />
            <div className={styles.breakdownItem}>
              <span className={styles.breakdownLabel}>ROLLOVER</span>
              <span className={`${styles.breakdownValue} ${Number(period?.rollover) >= 0 ? styles.success : styles.warning}`}>
                {formatCOP(Number(period?.rollover))}
              </span>
            </div>
            <div className={styles.breakdownDivider} />
            <div className={styles.breakdownItem}>
              <span className={styles.breakdownLabel}>RESTANTE</span>
              <span className={`${styles.breakdownValue} ${remaining >= 0 ? styles.success : styles.warning}`}>
                {formatCOP(remaining)}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* KPI row */}
      <div className={styles.kpiRow}>
        <div className={styles.kpiCard}>
          <div className={styles.kpiLabel}>GASTOS TOTALES</div>
          <div className={`${styles.kpiValue} ${styles.warning}`}>{formatCOP(total_expenses)}</div>
          <div className={styles.kpiNote}>{pct(total_expenses, available)}% del disponible</div>
        </div>
        <div className={styles.kpiCard}>
          <div className={styles.kpiLabel}>PRESUPUESTO RESTANTE</div>
          <div className={`${styles.kpiValue} ${budgetRemain >= 0 ? styles.success : styles.warning}`}>
            {formatCOP(Math.abs(budgetRemain))}
          </div>
          <div className={styles.kpiNote}>{budgetRemain >= 0 ? 'Control actual' : 'Sobre presupuesto'}</div>
        </div>
        <div className={styles.kpiCard}>
          <div className={styles.kpiLabel}>AHORRO ACUMULADO</div>
          <div className={styles.kpiValue}>
            {formatCOP(
              data.expenses_by_category.find(c => c.category_name === 'Ahorro')?.total
                ? Number(data.expenses_by_category.find(c => c.category_name === 'Ahorro')!.total)
                : 0
            )}
          </div>
          <div className={styles.kpiNote}>esta quincena</div>
        </div>
        <div className={styles.kpiCard}>
          <div className={styles.kpiLabel}>INVERSIÓN EJECUTADA</div>
          <div className={styles.kpiValue}>
            {formatCOP(
              data.expenses_by_category.find(c => c.category_name === 'Inversión')?.total
                ? Number(data.expenses_by_category.find(c => c.category_name === 'Inversión')!.total)
                : 0
            )}
          </div>
          <div className={styles.kpiNote}>esta quincena</div>
        </div>
      </div>

      {/* Main grid: charts + próximos pagos */}
      <div className={styles.mainGrid}>

        {/* Charts */}
        <div className={styles.chartsCol}>
          <div className={styles.sectionCard}>
            <h2 className={styles.sectionTitle}>Gastos por categoría</h2>
            {pieData.length > 0 ? (
              <>
                <div style={{ position: 'relative', width: '100%', height: 180 }}>
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie data={pieData} dataKey="total" nameKey="category_name"
                        cx="50%" cy="50%" outerRadius={70} paddingAngle={2} isAnimationActive={false}>
                        {pieData.map((e, i) => <Cell key={i} fill={e.category_color} />)}
                      </Pie>
                      <Tooltip formatter={(v: number) => formatCOP(v)} />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
                <div className={styles.legend}>
                  {pieData.map((c, i) => (
                    <div key={i} className={styles.legendItem}>
                      <span className={styles.dot} style={{ background: c.category_color }} />
                      <span>{c.category_name}</span>
                      <span className={styles.legendAmt}>{formatCOP(Number(c.total))}</span>
                    </div>
                  ))}
                </div>
              </>
            ) : <p className={styles.empty}>Sin gastos registrados</p>}
          </div>

          <div className={styles.sectionCard}>
            <h2 className={styles.sectionTitle}>Presupuesto vs Gasto</h2>
            {barData.some(d => d.Gasto > 0 || d.Presupuesto > 0) ? (
              <div style={{ position: 'relative', width: '100%', height: 200 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={barData} barSize={10} barCategoryGap="30%">
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" vertical={false} />
                    <XAxis dataKey="name" tick={{ fontSize: 10, fill: 'var(--color-text-muted)' }} />
                    <YAxis tick={{ fontSize: 10, fill: 'var(--color-text-muted)' }}
                      tickFormatter={v => `$${(v / 1000).toFixed(0)}k`} />
                    <Tooltip formatter={(v: number) => formatCOP(v)} />
                    <Legend wrapperStyle={{ fontSize: 11 }} />
                    <Bar dataKey="Presupuesto" fill="var(--color-border)" radius={[3, 3, 0, 0]} />
                    <Bar dataKey="Gasto" fill="var(--color-primary)" radius={[3, 3, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            ) : <p className={styles.empty}>Sin datos de presupuesto</p>}
          </div>
        </div>

        {/* Próximos pagos */}
        <div className={styles.upcomingCol}>
          <div className={styles.sectionCard}>
            <h2 className={styles.sectionTitle}>Próximos pagos</h2>
            {data.upcoming_payments.length === 0
              ? <p className={styles.empty}>Sin pagos próximos</p>
              : data.upcoming_payments.map(p => (
                <div key={p.id} className={styles.paymentItem}>
                  <div className={styles.paymentInfo}>
                    <div className={styles.paymentName}>{p.name}</div>
                    <div className={styles.paymentMeta}>
                      <span style={{ color: p.category_color }}>{p.category_name}</span>
                      <span className={styles.paymentDot}>·</span>
                      <span>vence {formatShortDate(p.due_date)}</span>
                    </div>
                  </div>
                  <div className={styles.paymentRight}>
                    <div className={styles.paymentAmount}>{formatCOP(Number(p.amount))}</div>
                    <Badge variant={p.status === 'paid' ? 'success' : p.status === 'overdue' ? 'danger' : 'default'}>
                      {p.status === 'paid' ? 'Pagado' : p.status === 'overdue' ? 'Vencido' : 'Pendiente'}
                    </Badge>
                  </div>
                </div>
              ))
            }
          </div>
        </div>
      </div>

      {/* Modal agregar gasto rápido */}
      <Modal open={txModal} onClose={() => setTxModal(false)} title="Agregar gasto">
        <form onSubmit={handleAddTx} className={styles.txForm}>
          <div className={styles.txRow}>
            <FormField label="Monto">
              <input type="number" min="0" step="1" placeholder="0"
                value={txForm.amount}
                onChange={e => setTxForm(f => ({ ...f, amount: e.target.value }))} required />
            </FormField>
            <FormField label="Fecha">
              <input type="date" value={txForm.date}
                onChange={e => setTxForm(f => ({ ...f, date: e.target.value }))} required />
            </FormField>
          </div>

          <FormField label="Descripción">
            <input type="text" value={txForm.description}
              onChange={e => setTxForm(f => ({ ...f, description: e.target.value }))} required />
          </FormField>

          <div className={styles.txRow}>
            <FormField label="Categoría">
              <select value={txForm.category_id}
                onChange={e => setTxForm(f => ({ ...f, category_id: e.target.value }))} required>
                <option value="">Seleccionar...</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </FormField>
            <FormField label="Método">
              <select value={txForm.payment_method_id}
                onChange={e => setTxForm(f => ({ ...f, payment_method_id: e.target.value, card_id: '' }))} required>
                <option value="">Seleccionar...</option>
                {methods.map(m => <option key={m.id} value={m.id}>{m.name}</option>)}
              </select>
            </FormField>
          </div>

          {requiresCard && (
            <FormField label="Tarjeta">
              <select value={txForm.card_id}
                onChange={e => setTxForm(f => ({ ...f, card_id: e.target.value }))} required>
                <option value="">Seleccionar...</option>
                {cards.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </FormField>
          )}

          <div className={styles.txActions}>
            <Button type="button" variant="secondary" onClick={() => setTxModal(false)}>Cancelar</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
