import { useEffect, useState } from 'react'
import Header from '../../components/layout/Header'
import Card from '../../components/ui/Card'
import { getBudgets, upsertBudget, getCategories, getPeriods } from '../../lib/api'
import { formatCOP, pct, clamp } from '../../lib/utils'
import type { Budget, Period } from '../../types'
import styles from './Budget.module.css'

export default function Budget() {
  const [periods, setPeriods] = useState<Period[]>([])
  const [selectedId, setSelectedId] = useState('')
  const [budgets, setBudgets] = useState<Budget[]>([])
  const [editing, setEditing] = useState<Record<string, string>>({})

  useEffect(() => {
    Promise.all([getPeriods(), getCategories()]).then(([pds]) => {
      setPeriods(pds)
      if (pds.length > 0) setSelectedId(pds[0].id)
    })
  }, [])

  useEffect(() => {
    if (!selectedId) return
    getBudgets(selectedId).then(setBudgets)
  }, [selectedId])

  async function handleSave(b: Budget) {
    const newAmount = parseFloat(editing[b.category_id] || String(b.amount))
    await upsertBudget({ period_id: selectedId, category_id: b.category_id, amount: newAmount })
    getBudgets(selectedId).then(setBudgets)
    setEditing(e => { const n = { ...e }; delete n[b.category_id]; return n })
  }

  const period = periods.find(p => p.id === selectedId)
  const totalBudgeted = budgets.reduce((s, b) => s + Number(b.amount), 0)
  const totalSpent    = budgets.reduce((s, b) => s + Number(b.spent || 0), 0)

  return (
    <div>
      <Header title="Presupuesto" subtitle="Por categoría y quincena" />

      <div className={styles.toolbar}>
        <select value={selectedId} onChange={e => setSelectedId(e.target.value)}>
          {periods.map(p => <option key={p.id} value={p.id}>{p.label}</option>)}
        </select>
      </div>

      {period && (
        <div className={styles.summary}>
          <Card>
            <div className={styles.kicker}>Ingreso base</div>
            <div className={styles.bigNum}>{formatCOP(Number(period.base_income))}</div>
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

      <div className={styles.list}>
        {budgets.map(b => {
          const budget = Number(b.amount)
          const spent  = Number(b.spent || 0)
          const progress = budget > 0 ? clamp(pct(spent, budget), 0, 100) : 0
          const over  = spent > budget && budget > 0
          const editVal = editing[b.category_id] ?? String(budget)

          return (
            <Card key={b.id || b.category_id} className={styles.budgetCard}>
              <div className={styles.cardTop}>
                <div className={styles.catInfo}>
                  <span className={styles.dot} style={{ background: b.category_color }} />
                  <span className={styles.catName}>{b.category_name}</span>
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
                    onChange={e => setEditing(prev => ({ ...prev, [b.category_id]: e.target.value }))}
                    onBlur={() => handleSave(b)}
                    min="0"
                  />
                </div>
              </div>
              <div className={styles.progressBar}>
                <div
                  className={`${styles.progressFill} ${over ? styles.progressOver : ''}`}
                  style={{ width: `${progress}%`, background: b.category_color }}
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
      </div>
    </div>
  )
}
