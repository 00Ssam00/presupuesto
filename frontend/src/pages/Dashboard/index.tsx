import { useEffect, useRef, useState } from 'react'
import {
  PieChart, Pie, Cell, Tooltip, ResponsiveContainer,
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Legend
} from 'recharts'
import Badge from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import Modal from '../../components/ui/Modal'
import FormField from '../../components/ui/FormField'
import Skeleton from '../../components/ui/Skeleton'
import { getDashboard, createTransaction } from '../../lib/api'
import { useReferenceData } from '../../lib/ReferenceDataContext'
import { formatCOP, formatShortDate, pct, todayISO, clampDateToPeriod } from '../../lib/utils'
import type { DatosPanel } from '../../types'
import styles from './Dashboard.module.css'

const EMPTY_TX = {
  periodo_id: '', categoria_id: '', metodo_pago_id: '',
  tarjeta_id: '', tipo: 'expense' as const,
  monto: '', descripcion: '', fecha: todayISO(),
}

export default function Dashboard() {
  const { categories, methods, cards, periods, currentPeriod, loading: refLoading, error: refError } = useReferenceData()

  const [data, setData]           = useState<DatosPanel | null>(null)
  const [selectedId, setSelectedId] = useState('')
  const [loading, setLoading]     = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError]         = useState('')
  const selectedIdTouched = useRef(false)

  const [txModal, setTxModal]         = useState(false)
  const [txForm, setTxForm]           = useState({ ...EMPTY_TX })
  const [saving, setSaving]           = useState(false)

  useEffect(() => {
    if (refLoading) return
    if (!selectedIdTouched.current) {
      const id = currentPeriod?.id || periods[0]?.id || ''
      setSelectedId(id)
      if (!id) setLoading(false)
    }
  }, [refLoading, currentPeriod, periods])

  function selectPeriod(id: string) {
    selectedIdTouched.current = true
    setSelectedId(id)
  }

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
    setTxForm({ ...EMPTY_TX, periodo_id: selectedId, fecha: clampDateToPeriod(period) })
    setTxModal(true)
  }

  async function handleAddTx(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    try {
      await createTransaction({
        ...txForm,
        monto: parseFloat(txForm.monto),
        tarjeta_id: requiresCard ? txForm.tarjeta_id : undefined,
      })
      setTxModal(false)
      getDashboard(selectedId).then(setData)
    } finally {
      setSaving(false)
    }
  }

  const requiresCard = methods.find(m => m.id === txForm.metodo_pago_id)?.requiere_tarjeta
  const period = data?.periodo

  const periodLabel = (() => {
    if (!period) return ''
    const start = new Date(period.fecha_inicio)
    const end   = new Date(period.fecha_fin)
    return `${start.getUTCDate()}–${end.getUTCDate()}`
  })()

  const sortedPeriods = [...periods].sort(
    (a, b) => new Date(a.fecha_inicio).getTime() - new Date(b.fecha_inicio).getTime()
  )
  const currentIdx = sortedPeriods.findIndex(p => p.id === selectedId)
  const goPrev = () => { if (currentIdx > 0) selectPeriod(sortedPeriods[currentIdx - 1].id) }
  const goNext = () => { if (currentIdx < sortedPeriods.length - 1) selectPeriod(sortedPeriods[currentIdx + 1].id) }

  // `total` llega como string desde Postgres (NUMERIC) — Recharts necesita un
  // number en dataKey para calcular los ángulos del pie, si no, no dibuja nada.
  const pieData = (data?.gastos_por_categoria || [])
    .map(c => ({ ...c, total: Number(c.total) }))
    .filter(c => c.total > 0)
  const barData = (data?.gastos_por_categoria || []).map(c => ({
    name: c.nombre_categoria.length > 8 ? c.nombre_categoria.slice(0, 8) + '…' : c.nombre_categoria,
    Presupuesto: Number(c.presupuesto),
    Gasto: Number(c.total),
  }))

  const totalBudget   = (data?.gastos_por_categoria || []).reduce((s, c) => s + Number(c.presupuesto), 0)
  const budgetRemain  = totalBudget - (data?.total_gastos || 0)

  if (error || refError) return (
    <div className={styles.stateMsg} style={{ color: 'var(--color-warning)' }}>{error || refError}</div>
  )
  if (loading || refLoading) return <DashboardSkeleton />
  if (!data)   return <div className={styles.stateMsg}>Sin datos. Ejecuta el seed en la base de datos.</div>

  const { disponible: available, total_gastos: total_expenses, restante: remaining } = data

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
              onChange={e => selectPeriod(e.target.value)}
            >
              {sortedPeriods.map(p => (
                <option key={p.id} value={p.id}>{p.etiqueta}</option>
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
              <span className={styles.breakdownValue}>{formatCOP(Number(period?.ingreso_base))}</span>
            </div>
            <div className={styles.breakdownDivider} />
            <div className={styles.breakdownItem}>
              <span className={styles.breakdownLabel}>ROLLOVER</span>
              <span className={`${styles.breakdownValue} ${Number(period?.arrastre) >= 0 ? styles.success : styles.warning}`}>
                {formatCOP(Number(period?.arrastre))}
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
              data.gastos_por_categoria.find(c => c.nombre_categoria === 'Ahorro')?.total
                ? Number(data.gastos_por_categoria.find(c => c.nombre_categoria === 'Ahorro')!.total)
                : 0
            )}
          </div>
          <div className={styles.kpiNote}>esta quincena</div>
        </div>
        <div className={styles.kpiCard}>
          <div className={styles.kpiLabel}>INVERSIÓN EJECUTADA</div>
          <div className={styles.kpiValue}>
            {formatCOP(
              data.gastos_por_categoria.find(c => c.nombre_categoria === 'Inversión')?.total
                ? Number(data.gastos_por_categoria.find(c => c.nombre_categoria === 'Inversión')!.total)
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
                      <Pie data={pieData} dataKey="total" nameKey="nombre_categoria"
                        cx="50%" cy="50%" outerRadius={70} paddingAngle={2} isAnimationActive={false}>
                        {pieData.map((e, i) => <Cell key={i} fill={e.color_categoria} />)}
                      </Pie>
                      <Tooltip formatter={(v: number) => formatCOP(v)} />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
                <div className={styles.legend}>
                  {pieData.map((c, i) => (
                    <div key={i} className={styles.legendItem}>
                      <span className={styles.dot} style={{ background: c.color_categoria }} />
                      <span>{c.nombre_categoria}</span>
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
            {data.pagos_proximos.length === 0
              ? <p className={styles.empty}>Sin pagos próximos</p>
              : data.pagos_proximos.map(p => (
                <div key={p.id} className={styles.paymentItem}>
                  <div className={styles.paymentInfo}>
                    <div className={styles.paymentName}>{p.nombre}</div>
                    <div className={styles.paymentMeta}>
                      <span style={{ color: p.color_categoria }}>{p.nombre_categoria}</span>
                      <span className={styles.paymentDot}>·</span>
                      <span>vence {formatShortDate(p.fecha_vencimiento)}</span>
                    </div>
                  </div>
                  <div className={styles.paymentRight}>
                    <div className={styles.paymentAmount}>{formatCOP(Number(p.monto))}</div>
                    <Badge variant={p.estado === 'paid' ? 'success' : p.estado === 'overdue' ? 'danger' : 'default'}>
                      {p.estado === 'paid' ? 'Pagado' : p.estado === 'overdue' ? 'Vencido' : 'Pendiente'}
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
                value={txForm.monto}
                onChange={e => setTxForm(f => ({ ...f, monto: e.target.value }))} required />
            </FormField>
            <FormField label="Fecha">
              <input type="date" value={txForm.fecha}
                onChange={e => setTxForm(f => ({ ...f, fecha: e.target.value }))} required />
            </FormField>
          </div>

          <FormField label="Descripción">
            <input type="text" value={txForm.descripcion}
              onChange={e => setTxForm(f => ({ ...f, descripcion: e.target.value }))} required />
          </FormField>

          <div className={styles.txRow}>
            <FormField label="Categoría">
              <select value={txForm.categoria_id}
                onChange={e => setTxForm(f => ({ ...f, categoria_id: e.target.value }))} required>
                <option value="">Seleccionar...</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
              </select>
            </FormField>
            <FormField label="Método">
              <select value={txForm.metodo_pago_id}
                onChange={e => setTxForm(f => ({ ...f, metodo_pago_id: e.target.value, tarjeta_id: '' }))} required>
                <option value="">Seleccionar...</option>
                {methods.map(m => <option key={m.id} value={m.id}>{m.nombre}</option>)}
              </select>
            </FormField>
          </div>

          {requiresCard && (
            <FormField label="Tarjeta">
              <select value={txForm.tarjeta_id}
                onChange={e => setTxForm(f => ({ ...f, tarjeta_id: e.target.value }))} required>
                <option value="">Seleccionar...</option>
                {cards.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
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

function DashboardSkeleton() {
  return (
    <div>
      <div className={styles.pageHeader}>
        <div>
          <h1 className={styles.pageTitle}>Dashboard</h1>
          <p className={styles.pageSubtitle}>Saldo disponible, rollover, presupuesto y pagos próximos.</p>
        </div>
        <Skeleton width={180} height={38} radius="var(--radius-lg)" />
      </div>

      <div className={styles.heroCard}>
        <Skeleton width={120} height={14} />
        <Skeleton width={260} height={48} className={styles.skeletonMt} />
        <Skeleton width={200} height={14} className={styles.skeletonMt} />
        <div className={styles.heroBreakdown} style={{ marginTop: 'var(--space-5)' }}>
          {[0, 1, 2].map(i => (
            <div key={i} className={styles.breakdownItem}>
              <Skeleton width={70} height={11} />
              <Skeleton width={90} height={20} />
            </div>
          ))}
        </div>
      </div>

      <div className={styles.kpiRow}>
        {[0, 1, 2, 3].map(i => (
          <div key={i} className={styles.kpiCard}>
            <Skeleton width={100} height={11} />
            <Skeleton width={80} height={22} className={styles.skeletonMt} />
            <Skeleton width={60} height={11} className={styles.skeletonMt} />
          </div>
        ))}
      </div>

      <div className={styles.mainGrid}>
        <div className={styles.chartsCol}>
          <div className={styles.sectionCard}>
            <Skeleton width={160} height={16} className={styles.skeletonMb} />
            <Skeleton width={180} height={180} radius="50%" className={styles.skeletonCenter} />
          </div>
          <div className={styles.sectionCard}>
            <Skeleton width={180} height={16} className={styles.skeletonMb} />
            <Skeleton width="100%" height={200} />
          </div>
        </div>
        <div className={styles.upcomingCol}>
          <div className={styles.sectionCard}>
            <Skeleton width={140} height={16} className={styles.skeletonMb} />
            {[0, 1, 2].map(i => (
              <div key={i} className={styles.paymentItem}>
                <div style={{ flex: 1 }}>
                  <Skeleton width="70%" height={14} />
                  <Skeleton width="50%" height={11} className={styles.skeletonMt} />
                </div>
                <Skeleton width={60} height={14} />
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
