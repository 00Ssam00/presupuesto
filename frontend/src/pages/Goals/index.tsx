import { useEffect, useState } from 'react'
import Header from '../../components/layout/Header'
import Card from '../../components/ui/Card'
import Badge from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import Modal from '../../components/ui/Modal'
import FormField from '../../components/ui/FormField'
import Skeleton from '../../components/ui/Skeleton'
import { obtenerMetas, crearMeta, actualizarMeta, eliminarMeta } from '../../lib/api'
import { formatCOP, pct, clamp } from '../../lib/utils'
import type { Meta } from '../../types'
import styles from './Goals.module.css'

const EMPTY = { nombre: '', tipo: 'savings' as 'savings' | 'investment', monto_objetivo: '', monto_actual: '', fecha_limite: '' }

export default function Goals() {
  const [goals, setGoals] = useState<Meta[]>([])
  const [loading, setLoading] = useState(true)
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<Meta | null>(null)
  const [form, setForm] = useState({ ...EMPTY })

  useEffect(() => { obtenerMetas().then(setGoals).finally(() => setLoading(false)) }, [])

  function openNew() { setEditing(null); setForm({ ...EMPTY }); setModalOpen(true) }
  function openEdit(g: Meta) {
    setEditing(g)
    setForm({
      nombre: g.nombre, tipo: g.tipo,
      monto_objetivo: String(g.monto_objetivo),
      monto_actual: String(g.monto_actual),
      fecha_limite: g.fecha_limite?.split('T')[0] || '',
    })
    setModalOpen(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const payload = {
      ...form,
      monto_objetivo: parseFloat(form.monto_objetivo),
      monto_actual: parseFloat(form.monto_actual || '0'),
      fecha_limite: form.fecha_limite || null,
    }
    if (editing) { await actualizarMeta(editing.id, payload) }
    else { await crearMeta(payload) }
    setModalOpen(false)
    obtenerMetas().then(setGoals)
  }

  async function handleDelete(id: string) {
    if (!confirm('¿Eliminar esta meta?')) return
    await eliminarMeta(id)
    setGoals(prev => prev.filter(g => g.id !== id))
  }

  const savings    = goals.filter(g => g.tipo === 'savings')
  const investment = goals.filter(g => g.tipo === 'investment')

  function GoalCard({ g }: { g: Meta }) {
    const progress = clamp(pct(Number(g.monto_actual), Number(g.monto_objetivo)), 0, 100)
    return (
      <Card className={styles.goalCard}>
        <div className={styles.goalTop}>
          <div>
            <div className={styles.goalName}>{g.nombre}</div>
            {g.fecha_limite && <div className={styles.deadline}>Meta: {g.fecha_limite.split('T')[0]}</div>}
          </div>
          <div className={styles.goalActions}>
            <Button variant="ghost" size="sm" onClick={() => openEdit(g)}>Editar</Button>
            <Button variant="ghost" size="sm" onClick={() => handleDelete(g.id)}>✕</Button>
          </div>
        </div>
        <div className={styles.goalAmounts}>
          <span className={styles.current}>{formatCOP(Number(g.monto_actual))}</span>
          <span className={styles.muted}> de {formatCOP(Number(g.monto_objetivo))}</span>
        </div>
        <div className={styles.progressBar}>
          <div
            className={styles.progressFill}
            style={{
              width: `${progress}%`,
              background: g.tipo === 'savings' ? 'var(--color-success)' : 'var(--color-primary)',
            }}
          />
        </div>
        <div className={styles.goalBottom}>
          <span className={styles.muted}>{progress}% completado</span>
          <Badge variant={progress >= 100 ? 'success' : 'default'}>
            {progress >= 100 ? 'Completada' : `Faltan ${formatCOP(Number(g.monto_objetivo) - Number(g.monto_actual))}`}
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
        {loading
          ? <div className={styles.grid}><GoalCardSkeleton /></div>
          : savings.length === 0
            ? <p className={styles.empty}>Sin metas de ahorro</p>
            : <div className={styles.grid}>{savings.map(g => <GoalCard key={g.id} g={g} />)}</div>
        }
      </div>

      <div className={styles.section}>
        <h2 className={styles.sectionTitle}>Inversión</h2>
        {loading
          ? <div className={styles.grid}><GoalCardSkeleton /></div>
          : investment.length === 0
            ? <p className={styles.empty}>Sin metas de inversión</p>
            : <div className={styles.grid}>{investment.map(g => <GoalCard key={g.id} g={g} />)}</div>
        }
      </div>

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editing ? 'Editar meta' : 'Nueva meta'}>
        <form onSubmit={handleSubmit} className={styles.form}>
          <FormField label="Nombre">
            <input type="text" value={form.nombre} onChange={e => setForm(f => ({ ...f, nombre: e.target.value }))} required />
          </FormField>
          <FormField label="Tipo">
            <select value={form.tipo} onChange={e => setForm(f => ({ ...f, tipo: e.target.value as 'savings' | 'investment' }))}>
              <option value="savings">Ahorro</option>
              <option value="investment">Inversión</option>
            </select>
          </FormField>
          <div className={styles.row}>
            <FormField label="Meta">
              <input type="number" min="0" value={form.monto_objetivo} onChange={e => setForm(f => ({ ...f, monto_objetivo: e.target.value }))} required />
            </FormField>
            <FormField label="Actual">
              <input type="number" min="0" value={form.monto_actual} onChange={e => setForm(f => ({ ...f, monto_actual: e.target.value }))} />
            </FormField>
          </div>
          <FormField label="Fecha límite (opcional)">
            <input type="date" value={form.fecha_limite} onChange={e => setForm(f => ({ ...f, fecha_limite: e.target.value }))} />
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

function GoalCardSkeleton() {
  return (
    <Card className={styles.goalCard}>
      <div className={styles.goalTop}>
        <div>
          <Skeleton width={140} height={16} />
          <Skeleton width={90} height={12} className={styles.skeletonMt} />
        </div>
      </div>
      <Skeleton width={160} height={20} className={styles.skeletonMt} />
      <div className={`${styles.progressBar} ${styles.skeletonMt}`}>
        <Skeleton width="100%" height="100%" radius="999px" />
      </div>
      <div className={styles.goalBottom}>
        <Skeleton width={100} height={12} />
        <Skeleton width={80} height={20} radius="var(--radius-sm)" />
      </div>
    </Card>
  )
}
