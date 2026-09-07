import { useEffect, useRef, useState } from 'react'
import Header from '../../components/layout/Header'
import Card from '../../components/ui/Card'
import Badge from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import Modal from '../../components/ui/Modal'
import FormField from '../../components/ui/FormField'
import Skeleton from '../../components/ui/Skeleton'
import { getTransactions, createTransaction, updateTransaction, deleteTransaction } from '../../lib/api'
import { useReferenceData } from '../../lib/ReferenceDataContext'
import { formatCOP, formatDate, todayISO } from '../../lib/utils'
import type { Transaccion } from '../../types'
import styles from './Transactions.module.css'

const EMPTY_FORM = {
  periodo_id: '', categoria_id: '', metodo_pago_id: '',
  tarjeta_id: '', tipo: 'expense' as 'income' | 'expense',
  monto: '', descripcion: '', nota: '', fecha: todayISO(),
}

export default function Transactions() {
  const { categories, methods, cards, periods, currentPeriod, loading: refLoading } = useReferenceData()

  const [transactions, setTransactions] = useState<Transaccion[]>([])
  const [txLoading, setTxLoading] = useState(true)

  const [filterPeriod, setFilterPeriod] = useState('')
  const [filterType, setFilterType] = useState('')
  const filterPeriodTouched = useRef(false)

  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<Transaccion | null>(null)
  const [form, setForm] = useState({ ...EMPTY_FORM })

  useEffect(() => {
    if (refLoading) return
    if (!filterPeriodTouched.current) {
      const id = currentPeriod?.id || periods[0]?.id || ''
      if (id) setFilterPeriod(id)
    }
  }, [refLoading, currentPeriod, periods])

  useEffect(() => {
    if (refLoading) return
    let cancelled = false
    setTxLoading(true)
    const params: Record<string, string> = {}
    if (filterPeriod) params.periodo_id = filterPeriod
    if (filterType)   params.tipo       = filterType
    getTransactions(params).then(data => {
      if (!cancelled) { setTransactions(data); setTxLoading(false) }
    })
    return () => { cancelled = true }
  }, [refLoading, filterPeriod, filterType])

  const requiresCard = methods.find(m => m.id === form.metodo_pago_id)?.requiere_tarjeta

  function openNew() {
    setEditing(null)
    setForm({ ...EMPTY_FORM, periodo_id: filterPeriod })
    setModalOpen(true)
  }

  function openEdit(t: Transaccion) {
    setEditing(t)
    setForm({
      periodo_id: t.periodo_id,
      categoria_id: t.categoria_id,
      metodo_pago_id: t.metodo_pago_id,
      tarjeta_id: t.tarjeta_id || '',
      tipo: t.tipo,
      monto: String(t.monto),
      descripcion: t.descripcion,
      nota: t.nota || '',
      fecha: t.fecha.split('T')[0],
    })
    setModalOpen(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const payload = {
      ...form,
      monto: parseFloat(form.monto),
      tarjeta_id: requiresCard ? form.tarjeta_id : null,
    }
    if (editing) {
      await updateTransaction(editing.id, payload)
    } else {
      await createTransaction(payload)
    }
    setModalOpen(false)
    const params: Record<string, string> = {}
    if (filterPeriod) params.periodo_id = filterPeriod
    if (filterType)   params.tipo       = filterType
    getTransactions(params).then(setTransactions)
  }

  async function handleDelete(id: string) {
    if (!confirm('¿Eliminar esta transacción?')) return
    await deleteTransaction(id)
    setTransactions(prev => prev.filter(t => t.id !== id))
  }

  return (
    <div>
      <Header title="Transacciones" />

      <div className={styles.toolbar}>
        <div className={styles.filters}>
          <select value={filterPeriod} onChange={e => { filterPeriodTouched.current = true; setFilterPeriod(e.target.value) }}>
            <option value="">Todas las quincenas</option>
            {periods.map(p => <option key={p.id} value={p.id}>{p.etiqueta}</option>)}
          </select>
          <select value={filterType} onChange={e => setFilterType(e.target.value)}>
            <option value="">Todos los tipos</option>
            <option value="income">Ingresos</option>
            <option value="expense">Gastos</option>
          </select>
        </div>
        <Button onClick={openNew}>+ Nueva transacción</Button>
      </div>

      <Card>
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Descripción</th>
              <th>Categoría</th>
              <th>Método</th>
              <th>Fecha</th>
              <th className={styles.right}>Monto</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {txLoading && [0, 1, 2, 3, 4, 5].map(i => (
              <tr key={`sk-${i}`}>
                <td><Skeleton width="70%" height={14} /></td>
                <td><Skeleton width={80} height={20} radius="var(--radius-sm)" /></td>
                <td><Skeleton width="60%" height={14} /></td>
                <td><Skeleton width={90} height={14} /></td>
                <td className={styles.right}><Skeleton width={70} height={14} className={styles.skeletonRight} /></td>
                <td></td>
              </tr>
            ))}
            {!txLoading && transactions.length === 0 && (
              <tr><td colSpan={6} className={styles.empty}>Sin transacciones</td></tr>
            )}
            {!txLoading && transactions.map(t => (
              <tr key={t.id}>
                <td>
                  <div className={styles.desc}>{t.descripcion}</div>
                  {t.nota && <div className={styles.note}>{t.nota}</div>}
                </td>
                <td>
                  <Badge style={{ background: `${t.color_categoria}22`, color: t.color_categoria }}>
                    {t.nombre_categoria}
                  </Badge>
                </td>
                <td className={styles.muted}>
                  {t.nombre_metodo_pago}
                  {t.nombre_tarjeta && ` · ${t.nombre_tarjeta}`}
                </td>
                <td className={styles.muted}>{formatDate(t.fecha)}</td>
                <td className={`${styles.right} ${t.tipo === 'income' ? styles.success : styles.warning}`}>
                  {t.tipo === 'income' ? '+' : '-'}{formatCOP(Number(t.monto))}
                </td>
                <td>
                  <div className={styles.actions}>
                    <Button variant="ghost" size="sm" onClick={() => openEdit(t)}>Editar</Button>
                    <Button variant="ghost" size="sm" onClick={() => handleDelete(t.id)}>✕</Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={editing ? 'Editar transacción' : 'Nueva transacción'}
      >
        <form onSubmit={handleSubmit} className={styles.form}>
          <FormField label="Quincena">
            <select value={form.periodo_id} onChange={e => setForm(f => ({ ...f, periodo_id: e.target.value }))} required>
              <option value="">Seleccionar...</option>
              {periods.map(p => <option key={p.id} value={p.id}>{p.etiqueta}</option>)}
            </select>
          </FormField>

          <div className={styles.row}>
            <FormField label="Tipo">
              <select value={form.tipo} onChange={e => setForm(f => ({ ...f, tipo: e.target.value as 'income' | 'expense' }))} required>
                <option value="expense">Gasto</option>
                <option value="income">Ingreso</option>
              </select>
            </FormField>
            <FormField label="Monto">
              <input type="number" min="0" step="1" value={form.monto}
                onChange={e => setForm(f => ({ ...f, monto: e.target.value }))} required placeholder="0" />
            </FormField>
          </div>

          <FormField label="Descripción">
            <input type="text" value={form.descripcion}
              onChange={e => setForm(f => ({ ...f, descripcion: e.target.value }))} required />
          </FormField>

          <div className={styles.row}>
            <FormField label="Categoría">
              <select value={form.categoria_id} onChange={e => setForm(f => ({ ...f, categoria_id: e.target.value }))} required>
                <option value="">Seleccionar...</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
              </select>
            </FormField>
            <FormField label="Fecha">
              <input type="date" value={form.fecha}
                onChange={e => setForm(f => ({ ...f, fecha: e.target.value }))} required />
            </FormField>
          </div>

          <div className={styles.row}>
            <FormField label="Método de pago">
              <select value={form.metodo_pago_id}
                onChange={e => setForm(f => ({ ...f, metodo_pago_id: e.target.value, tarjeta_id: '' }))} required>
                <option value="">Seleccionar...</option>
                {methods.map(m => <option key={m.id} value={m.id}>{m.nombre}</option>)}
              </select>
            </FormField>
            {requiresCard && (
              <FormField label="Tarjeta">
                <select value={form.tarjeta_id} onChange={e => setForm(f => ({ ...f, tarjeta_id: e.target.value }))} required>
                  <option value="">Seleccionar...</option>
                  {cards.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
                </select>
              </FormField>
            )}
          </div>

          <FormField label="Nota (opcional)">
            <input type="text" value={form.nota}
              onChange={e => setForm(f => ({ ...f, nota: e.target.value }))} />
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
