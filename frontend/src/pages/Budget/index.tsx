import { useEffect, useRef, useState } from 'react'
import Header from '../../components/layout/Header'
import Card from '../../components/ui/Card'
import Skeleton from '../../components/ui/Skeleton'
import { getBudgets, upsertBudget } from '../../lib/api'
import { useReferenceData } from '../../lib/ReferenceDataContext'
import { formatCOP, pct, clamp } from '../../lib/utils'
import type { Presupuesto } from '../../types'
import styles from './Budget.module.css'

export default function Budget() {
  const { periods, currentPeriod, loading: refLoading } = useReferenceData()
  const [selectedId, setSelectedId] = useState('')
  const [budgets, setBudgets] = useState<Presupuesto[]>([])
  const [budgetsLoading, setBudgetsLoading] = useState(true)
  const [editing, setEditing] = useState<Record<string, string>>({})
  const selectedIdTouched = useRef(false)

  useEffect(() => {
    if (refLoading) return
    if (!selectedIdTouched.current) {
      const id = currentPeriod?.id || periods[0]?.id || ''
      if (id) setSelectedId(id)
    }
  }, [refLoading, currentPeriod, periods])

  useEffect(() => {
    if (!selectedId) return
    let cancelled = false
    setBudgetsLoading(true)
    getBudgets(selectedId).then(data => {
      if (!cancelled) { setBudgets(data); setBudgetsLoading(false) }
    })
    return () => { cancelled = true }
  }, [selectedId])

  async function handleSave(b: Presupuesto) {
    const newAmount = parseFloat(editing[b.categoria_id] || String(b.monto))
    await upsertBudget({ periodo_id: selectedId, categoria_id: b.categoria_id, monto: newAmount })
    getBudgets(selectedId).then(setBudgets)
    setEditing(e => { const n = { ...e }; delete n[b.categoria_id]; return n })
  }

  const period = periods.find(p => p.id === selectedId)
  const totalBudgeted = budgets.reduce((s, b) => s + Number(b.monto), 0)
  const totalSpent    = budgets.reduce((s, b) => s + Number(b.gastado || 0), 0)
  const loading = refLoading || budgetsLoading

  return (
    <div>
      <Header title="Presupuesto" subtitle="Por categoría y quincena" />

      <div className={styles.toolbar}>
        {refLoading
          ? <Skeleton width={180} height={38} radius="var(--radius-lg)" />
          : (
            <select value={selectedId} onChange={e => { selectedIdTouched.current = true; setSelectedId(e.target.value) }}>
              {periods.map(p => <option key={p.id} value={p.id}>{p.etiqueta}</option>)}
            </select>
          )}
      </div>

      {loading && (
        <div className={styles.summary}>
          {[0, 1, 2].map(i => (
            <Card key={i}>
              <Skeleton width={100} height={12} />
              <Skeleton width={130} height={26} className={styles.skeletonMt} />
            </Card>
          ))}
        </div>
      )}

      {!loading && period && (
        <div className={styles.summary}>
          <Card>
            <div className={styles.kicker}>Ingreso base</div>
            <div className={styles.bigNum}>{formatCOP(Number(period.ingreso_base))}</div>
          </Card>
          <Card>
            <div className={styles.kicker}>Total presupuestado</div>
            <div className={styles.bigNum}>{formatCOP(totalBudgeted)}</div>
          </Card>
          <Card>
            <div className={styles.kicker}>Total gastado</div>
            <div className={`${styles.bigNum} ${styles.warning}`}>{formatCOP(totalSpent)}</div>
          </Card>
        </div>
      )}

      {loading && (
        <div className={styles.list}>
          {[0, 1, 2, 3, 4].map(i => (
            <Card key={i} className={styles.budgetCard}>
              <div className={styles.cardTop}>
                <div className={styles.catInfo}>
                  <Skeleton width={9} height={9} radius="50%" />
                  <Skeleton width={100} height={14} />
                </div>
                <Skeleton width={110} height={14} />
              </div>
              <Skeleton width="100%" height={8} radius="var(--radius-sm)" className={styles.skeletonMt} />
            </Card>
          ))}
        </div>
      )}

      {!loading && <div className={styles.list}>
        {budgets.map(b => {
          const budget = Number(b.monto)
          const spent  = Number(b.gastado || 0)
          const progress = budget > 0 ? clamp(pct(spent, budget), 0, 100) : 0
          const over  = spent > budget && budget > 0
          const editVal = editing[b.categoria_id] ?? String(budget)

          return (
            <Card key={b.id || b.categoria_id} className={styles.budgetCard}>
              <div className={styles.cardTop}>
                <div className={styles.catInfo}>
                  <span className={styles.dot} style={{ background: b.color_categoria }} />
                  <span className={styles.catName}>{b.nombre_categoria}</span>
                </div>
                <div className={styles.amounts}>
                  <span className={over ? styles.warning : styles.success}>
                    {formatCOP(spent)}
                  </span>
                  <span className={styles.muted}> / </span>
                  <input
                    type="number"
                    className={styles.budgetInput}
                    value={editVal}
                    onChange={e => setEditing(prev => ({ ...prev, [b.categoria_id]: e.target.value }))}
                    onBlur={() => handleSave(b)}
                    min="0"
                  />
                </div>
              </div>
              <div className={styles.progressBar}>
                <div
                  className={`${styles.progressFill} ${over ? styles.progressOver : ''}`}
                  style={{ width: `${progress}%`, background: b.color_categoria }}
                />
              </div>
              <div className={styles.cardBottom}>
                <span className={styles.muted}>{progress}% usado</span>
                {over && <span className={styles.warning}>+{formatCOP(spent - budget)} sobre el límite</span>}
              </div>
            </Card>
          )
        })}
        {budgets.length === 0 && (
          <p className={styles.empty}>No hay presupuesto configurado para esta quincena.</p>
        )}
      </div>}
    </div>
  )
}
