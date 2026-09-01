import { useEffect, useState } from 'react'
import Header from '../../components/layout/Header'
import Card from '../../components/ui/Card'
import Badge from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import Modal from '../../components/ui/Modal'
import FormField from '../../components/ui/FormField'
import { getGoals, createGoal, updateGoal, deleteGoal } from '../../lib/api'
import { formatCOP, pct, clamp } from '../../lib/utils'
import type { Goal } from '../../types'
import styles from './Goals.module.css'

const EMPTY = { name: '', type: 'savings' as 'savings' | 'investment', target_amount: '', current_amount: '', deadline: '' }

export default function Goals() {
  const [goals, setGoals] = useState<Goal[]>([])
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<Goal | null>(null)
  const [form, setForm] = useState({ ...EMPTY })

  useEffect(() => { getGoals().then(setGoals) }, [])

  function openNew() { setEditing(null); setForm({ ...EMPTY }); setModalOpen(true) }
  function openEdit(g: Goal) {
    setEditing(g)
    setForm({
      name: g.name, type: g.type,
      target_amount: String(g.target_amount),
      current_amount: String(g.current_amount),
      deadline: g.deadline?.split('T')[0] || '',
    })
    setModalOpen(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const payload = { ...form, target_amount: parseFloat(form.target_amount), current_amount: parseFloat(form.current_amount || '0'), deadline: form.deadline || null }
    if (editing) { await updateGoal(editing.id, payload) }
    else { await createGoal(payload) }
    setModalOpen(false)
    getGoals().then(setGoals)
  }

  async function handleDelete(id: string) {
    if (!confirm('¿Eliminar esta meta?')) return
    await deleteGoal(id)
    setGoals(prev => prev.filter(g => g.id !== id))
  }

  const savings    = goals.filter(g => g.type === 'savings')
  const investment = goals.filter(g => g.type === 'investment')

  function GoalCard({ g }: { g: Goal }) {
    const progress = clamp(pct(Number(g.current_amount), Number(g.target_amount)), 0, 100)
    return (
      <Card className={styles.goalCard}>
        <div className={styles.goalTop}>
          <div>
            <div className={styles.goalName}>{g.name}</div>
            {g.deadline && <div className={styles.deadline}>Meta: {g.deadline.split('T')[0]}</div>}
          </div>
          <div className={styles.goalActions}>
            <Button variant="ghost" size="sm" onClick={() => openEdit(g)}>Editar</Button>
            <Button variant="ghost" size="sm" onClick={() => handleDelete(g.id)}>✕</Button>
          </div>
        </div>
        <div className={styles.goalAmounts}>
          <span className={styles.current}>{formatCOP(Number(g.current_amount))}</span>
          <span className={styles.muted}> de {formatCOP(Number(g.target_amount))}</span>
        </div>
        <div className={styles.progressBar}>
          <div
            className={styles.progressFill}
            style={{
              width: `${progress}%`,
              background: g.type === 'savings' ? 'var(--color-success)' : 'var(--color-primary)',
            }}
          />
        </div>
        <div className={styles.goalBottom}>
          <span className={styles.muted}>{progress}% completado</span>
          <Badge variant={progress >= 100 ? 'success' : 'default'}>
            {progress >= 100 ? 'Completada' : `Faltan ${formatCOP(Number(g.target_amount) - Number(g.current_amount))}`}
          </Badge>
        </div>
      </Card>
    )
  }

  return (
    <div>
      <Header title="Metas" subtitle="Ahorro e inversión" />

      <div className={styles.toolbar}>
        <Button onClick={openNew}>+ Nueva meta</Button>
      </div>

      <div className={styles.section}>
        <h2 className={styles.sectionTitle}>Ahorro</h2>
        {savings.length === 0
          ? <p className={styles.empty}>Sin metas de ahorro</p>
          : <div className={styles.grid}>{savings.map(g => <GoalCard key={g.id} g={g} />)}</div>
        }
      </div>

      <div className={styles.section}>
        <h2 className={styles.sectionTitle}>Inversión</h2>
        {investment.length === 0
          ? <p className={styles.empty}>Sin metas de inversión</p>
          : <div className={styles.grid}>{investment.map(g => <GoalCard key={g.id} g={g} />)}</div>
        }
      </div>

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editing ? 'Editar meta' : 'Nueva meta'}>
        <form onSubmit={handleSubmit} className={styles.form}>
          <FormField label="Nombre">
            <input type="text" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required />
          </FormField>
          <FormField label="Tipo">
            <select value={form.type} onChange={e => setForm(f => ({ ...f, type: e.target.value as 'savings' | 'investment' }))}>
              <option value="savings">Ahorro</option>
              <option value="investment">Inversión</option>
            </select>
          </FormField>
          <div className={styles.row}>
            <FormField label="Meta">
              <input type="number" min="0" value={form.target_amount} onChange={e => setForm(f => ({ ...f, target_amount: e.target.value }))} required />
            </FormField>
            <FormField label="Actual">
              <input type="number" min="0" value={form.current_amount} onChange={e => setForm(f => ({ ...f, current_amount: e.target.value }))} />
            </FormField>
          </div>
          <FormField label="Fecha límite (opcional)">
            <input type="date" value={form.deadline} onChange={e => setForm(f => ({ ...f, deadline: e.target.value }))} />
          </FormField>
          <div className={styles.formActions}>
            <Button type="button" variant="secondary" onClick={() => setModalOpen(false)}>Cancelar</Button>
            <Button type="submit">{editing ? 'Guardar' : 'Crear'}</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
