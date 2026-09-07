import { useState } from 'react'
import Header from '../../components/layout/Header'
import Card from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Modal from '../../components/ui/Modal'
import FormField from '../../components/ui/FormField'
import Badge from '../../components/ui/Badge'
import Skeleton from '../../components/ui/Skeleton'
import {
  createCategory, updateCategory, deleteCategory,
  createCard, updateCard, deleteCard,
  createPeriod, deletePeriod,
} from '../../lib/api'
import { useReferenceData } from '../../lib/ReferenceDataContext'
import { formatDate } from '../../lib/utils'
import type { Categoria, Tarjeta } from '../../types'
import styles from './Settings.module.css'

export default function Settings() {
  const { categories, cards, periods, loading, refresh } = useReferenceData()

  const [catModal, setCatModal] = useState(false)
  const [editingCat, setEditingCat] = useState<Categoria | null>(null)
  const [catForm, setCatForm] = useState({ nombre: '', color: '#01696f' })

  const [cardModal, setCardModal] = useState(false)
  const [editingCard, setEditingCard] = useState<Tarjeta | null>(null)
  const [cardForm, setCardForm] = useState({ nombre: '', ultimos_cuatro: '', banco: '' })

  const [periodModal, setPeriodModal] = useState(false)
  const [periodForm, setPeriodForm] = useState({ etiqueta: '', fecha_inicio: '', fecha_fin: '', ingreso_base: '' })

  async function saveCat(e: React.FormEvent) {
    e.preventDefault()
    if (editingCat) { await updateCategory(editingCat.id, catForm) }
    else { await createCategory(catForm) }
    setCatModal(false)
    refresh()
  }

  async function saveCard(e: React.FormEvent) {
    e.preventDefault()
    if (editingCard) { await updateCard(editingCard.id, cardForm) }
    else { await createCard(cardForm) }
    setCardModal(false)
    refresh()
  }

  async function savePeriod(e: React.FormEvent) {
    e.preventDefault()
    await createPeriod({ ...periodForm, ingreso_base: parseFloat(periodForm.ingreso_base) })
    setPeriodModal(false)
    refresh()
  }

  return (
    <div>
      <Header title="Configuración" />

      <div className={styles.sections}>

        {/* Categorías */}
        <Card>
          <div className={styles.secHead}>
            <h2 className={styles.secTitle}>Categorías</h2>
            <Button size="sm" onClick={() => { setEditingCat(null); setCatForm({ nombre: '', color: '#01696f' }); setCatModal(true) }}>
              + Agregar
            </Button>
          </div>
          <div className={styles.list}>
            {loading && <ListSkeleton />}
            {!loading && categories.map(c => (
              <div key={c.id} className={styles.listItem}>
                <span className={styles.dot} style={{ background: c.color }} />
                <span className={styles.itemName}>{c.nombre}</span>
                <div className={styles.itemActions}>
                  <Button variant="ghost" size="sm" onClick={() => {
                    setEditingCat(c); setCatForm({ nombre: c.nombre, color: c.color }); setCatModal(true)
                  }}>Editar</Button>
                  <Button variant="ghost" size="sm" onClick={async () => {
                    if (!confirm('¿Desactivar?')) return
                    await deleteCategory(c.id)
                    refresh()
                  }}>✕</Button>
                </div>
              </div>
            ))}
          </div>
        </Card>

        {/* Tarjetas */}
        <Card>
          <div className={styles.secHead}>
            <h2 className={styles.secTitle}>Tarjetas de crédito</h2>
            <Button size="sm" onClick={() => { setEditingCard(null); setCardForm({ nombre: '', ultimos_cuatro: '', banco: '' }); setCardModal(true) }}>
              + Agregar
            </Button>
          </div>
          <div className={styles.list}>
            {loading && <ListSkeleton />}
            {!loading && cards.length === 0 && <p className={styles.empty}>Sin tarjetas registradas</p>}
            {!loading && cards.map(c => (
              <div key={c.id} className={styles.listItem}>
                <span className={styles.itemName}>{c.nombre}</span>
                {c.ultimos_cuatro && <Badge variant="default">···{c.ultimos_cuatro}</Badge>}
                {c.banco && <span className={styles.muted}>{c.banco}</span>}
                <div className={styles.itemActions}>
                  <Button variant="ghost" size="sm" onClick={() => {
                    setEditingCard(c); setCardForm({ nombre: c.nombre, ultimos_cuatro: c.ultimos_cuatro || '', banco: c.banco || '' }); setCardModal(true)
                  }}>Editar</Button>
                  <Button variant="ghost" size="sm" onClick={async () => {
                    if (!confirm('¿Desactivar?')) return
                    await deleteCard(c.id)
                    refresh()
                  }}>✕</Button>
                </div>
              </div>
            ))}
          </div>
        </Card>

        {/* Quincenas */}
        <Card>
          <div className={styles.secHead}>
            <h2 className={styles.secTitle}>Quincenas</h2>
            <Button size="sm" onClick={() => { setPeriodForm({ etiqueta: '', fecha_inicio: '', fecha_fin: '', ingreso_base: '' }); setPeriodModal(true) }}>
              + Nueva quincena
            </Button>
          </div>
          <div className={styles.list}>
            {loading && <ListSkeleton />}
            {!loading && periods.slice(0, 8).map(p => (
              <div key={p.id} className={styles.listItem}>
                <span className={styles.itemName}>{p.etiqueta}</span>
                <span className={styles.muted}>{formatDate(p.fecha_inicio)} — {formatDate(p.fecha_fin)}</span>
                {p.esta_cerrado && <Badge variant="warning">Cerrada</Badge>}
                <Button variant="ghost" size="sm" onClick={async () => {
                  if (!confirm('¿Eliminar quincena?')) return
                  await deletePeriod(p.id)
                  refresh()
                }}>✕</Button>
              </div>
            ))}
          </div>
        </Card>

      </div>

      {/* Modal categoría */}
      <Modal open={catModal} onClose={() => setCatModal(false)} title={editingCat ? 'Editar categoría' : 'Nueva categoría'}>
        <form onSubmit={saveCat} className={styles.form}>
          <FormField label="Nombre">
            <input type="text" value={catForm.nombre} onChange={e => setCatForm(f => ({ ...f, nombre: e.target.value }))} required />
          </FormField>
          <FormField label="Color">
            <input type="color" value={catForm.color} onChange={e => setCatForm(f => ({ ...f, color: e.target.value }))} style={{ height: 40 }} />
          </FormField>
          <div className={styles.formActions}>
            <Button type="button" variant="secondary" onClick={() => setCatModal(false)}>Cancelar</Button>
            <Button type="submit">Guardar</Button>
          </div>
        </form>
      </Modal>

      {/* Modal tarjeta */}
      <Modal open={cardModal} onClose={() => setCardModal(false)} title={editingCard ? 'Editar tarjeta' : 'Nueva tarjeta'}>
        <form onSubmit={saveCard} className={styles.form}>
          <FormField label="Nombre de la tarjeta">
            <input type="text" value={cardForm.nombre} onChange={e => setCardForm(f => ({ ...f, nombre: e.target.value }))} required />
          </FormField>
          <div className={styles.row}>
            <FormField label="Últimos 4 dígitos">
              <input type="text" maxLength={4} value={cardForm.ultimos_cuatro} onChange={e => setCardForm(f => ({ ...f, ultimos_cuatro: e.target.value }))} />
            </FormField>
            <FormField label="Banco">
              <input type="text" value={cardForm.banco} onChange={e => setCardForm(f => ({ ...f, banco: e.target.value }))} />
            </FormField>
          </div>
          <div className={styles.formActions}>
            <Button type="button" variant="secondary" onClick={() => setCardModal(false)}>Cancelar</Button>
            <Button type="submit">Guardar</Button>
          </div>
        </form>
      </Modal>

      {/* Modal quincena */}
      <Modal open={periodModal} onClose={() => setPeriodModal(false)} title="Nueva quincena">
        <form onSubmit={savePeriod} className={styles.form}>
          <FormField label="Etiqueta">
            <input type="text" placeholder="1ra quincena Mayo 2026" value={periodForm.etiqueta}
              onChange={e => setPeriodForm(f => ({ ...f, etiqueta: e.target.value }))} required />
          </FormField>
          <div className={styles.row}>
            <FormField label="Inicio">
              <input type="date" value={periodForm.fecha_inicio} onChange={e => setPeriodForm(f => ({ ...f, fecha_inicio: e.target.value }))} required />
            </FormField>
            <FormField label="Fin">
              <input type="date" value={periodForm.fecha_fin} onChange={e => setPeriodForm(f => ({ ...f, fecha_fin: e.target.value }))} required />
            </FormField>
          </div>
          <FormField label="Ingreso base">
            <input type="number" min="0" value={periodForm.ingreso_base}
              onChange={e => setPeriodForm(f => ({ ...f, ingreso_base: e.target.value }))} required />
          </FormField>
          <div className={styles.formActions}>
            <Button type="button" variant="secondary" onClick={() => setPeriodModal(false)}>Cancelar</Button>
            <Button type="submit">Crear</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}

function ListSkeleton() {
  return (
    <>
      {[0, 1, 2].map(i => (
        <div key={i} className={styles.listItem}>
          <Skeleton width={120} height={14} />
        </div>
      ))}
    </>
  )
}
