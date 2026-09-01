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
import type { Transaction } from '../../types'
import styles from './Transactions.module.css'

const EMPTY_FORM = {
  period_id: '', category_id: '', payment_method_id: '',
  card_id: '', type: 'expense' as 'income' | 'expense',
  amount: '', description: '', note: '', date: todayISO(),
}

export default function Transactions() {
  const { categories, methods, cards, periods, currentPeriod, loading: refLoading } = useReferenceData()

  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [txLoading, setTxLoading] = useState(true)

  const [filterPeriod, setFilterPeriod] = useState('')
  const [filterType, setFilterType] = useState('')
  const filterPeriodTouched = useRef(false)

  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<Transaction | null>(null)
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
    if (filterPeriod) params.period_id = filterPeriod
    if (filterType)   params.type      = filterType
    getTransactions(params).then(data => {
      if (!cancelled) { setTransactions(data); setTxLoading(false) }
    })
    return () => { cancelled = true }
  }, [refLoading, filterPeriod, filterType])

  const requiresCard = methods.find(m => m.id === form.payment_method_id)?.requires_card

  function openNew() {
    setEditing(null)
    setForm({ ...EMPTY_FORM, period_id: filterPeriod })
    setModalOpen(true)
  }

  function openEdit(t: Transaction) {
    setEditing(t)
    setForm({
      period_id: t.period_id,
      category_id: t.category_id,
      payment_method_id: t.payment_method_id,
      card_id: t.card_id || '',
      type: t.type,
      amount: String(t.amount),
      description: t.description,
      note: t.note || '',
      date: t.date.split('T')[0],
    })
    setModalOpen(true)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const payload = {
      ...form,
      amount: parseFloat(form.amount),
      card_id: requiresCard ? form.card_id : null,
    }
    if (editing) {
      await updateTransaction(editing.id, payload)
    } else {
      await createTransaction(payload)
    }
    setModalOpen(false)
    const params: Record<string, string> = {}
    if (filterPeriod) params.period_id = filterPeriod
    if (filterType)   params.type      = filterType
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
            {periods.map(p => <option key={p.id} value={p.id}>{p.label}</option>)}
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
                  <div className={styles.desc}>{t.description}</div>
                  {t.note && <div className={styles.note}>{t.note}</div>}
                </td>
                <td>
                  <Badge style={{ background: `${t.category_color}22`, color: t.category_color }}>
                    {t.category_name}
                  </Badge>
                </td>
                <td className={styles.muted}>
                  {t.payment_method_name}
                  {t.card_name && ` · ${t.card_name}`}
                </td>
                <td className={styles.muted}>{formatDate(t.date)}</td>
                <td className={`${styles.right} ${t.type === 'income' ? styles.success : styles.warning}`}>
                  {t.type === 'income' ? '+' : '-'}{formatCOP(Number(t.amount))}
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
            <select value={form.period_id} onChange={e => setForm(f => ({ ...f, period_id: e.target.value }))} required>
              <option value="">Seleccionar...</option>
              {periods.map(p => <option key={p.id} value={p.id}>{p.label}</option>)}
            </select>
          </FormField>

          <div className={styles.row}>
            <FormField label="Tipo">
              <select value={form.type} onChange={e => setForm(f => ({ ...f, type: e.target.value as 'income' | 'expense' }))} required>
                <option value="expense">Gasto</option>
                <option value="income">Ingreso</option>
              </select>
            </FormField>
            <FormField label="Monto">
              <input type="number" min="0" step="1" value={form.amount}
                onChange={e => setForm(f => ({ ...f, amount: e.target.value }))} required placeholder="0" />
            </FormField>
          </div>

          <FormField label="Descripción">
            <input type="text" value={form.description}
              onChange={e => setForm(f => ({ ...f, description: e.target.value }))} required />
          </FormField>

          <div className={styles.row}>
            <FormField label="Categoría">
              <select value={form.category_id} onChange={e => setForm(f => ({ ...f, category_id: e.target.value }))} required>
                <option value="">Seleccionar...</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </FormField>
            <FormField label="Fecha">
              <input type="date" value={form.date}
                onChange={e => setForm(f => ({ ...f, date: e.target.value }))} required />
            </FormField>
          </div>

          <div className={styles.row}>
            <FormField label="Método de pago">
              <select value={form.payment_method_id}
                onChange={e => setForm(f => ({ ...f, payment_method_id: e.target.value, card_id: '' }))} required>
                <option value="">Seleccionar...</option>
                {methods.map(m => <option key={m.id} value={m.id}>{m.name}</option>)}
              </select>
            </FormField>
            {requiresCard && (
              <FormField label="Tarjeta">
                <select value={form.card_id} onChange={e => setForm(f => ({ ...f, card_id: e.target.value }))} required>
                  <option value="">Seleccionar...</option>
                  {cards.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </FormField>
            )}
          </div>

          <FormField label="Nota (opcional)">
            <input type="text" value={form.note}
              onChange={e => setForm(f => ({ ...f, note: e.target.value }))} />
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
