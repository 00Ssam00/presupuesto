import { useEffect, useState } from 'react'
import Header from '../../components/layout/Header'
import Card from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Modal from '../../components/ui/Modal'
import FormField from '../../components/ui/FormField'
import Badge from '../../components/ui/Badge'
import {
  getCategories, createCategory, updateCategory, deleteCategory,
  getCards, createCard, updateCard, deleteCard,
  getPeriods, createPeriod, deletePeriod,
} from '../../lib/api'
import { formatDate } from '../../lib/utils'
import type { Category, Card as CardType, Period } from '../../types'
import styles from './Settings.module.css'

export default function Settings() {
  const [categories, setCategories] = useState<Category[]>([])
  const [cards, setCards] = useState<CardType[]>([])
  const [periods, setPeriods] = useState<Period[]>([])

  const [catModal, setCatModal] = useState(false)
  const [editingCat, setEditingCat] = useState<Category | null>(null)
  const [catForm, setCatForm] = useState({ name: '', color: '#01696f' })

  const [cardModal, setCardModal] = useState(false)
  const [editingCard, setEditingCard] = useState<CardType | null>(null)
  const [cardForm, setCardForm] = useState({ name: '', last_four: '', bank: '' })

  const [periodModal, setPeriodModal] = useState(false)
  const [periodForm, setPeriodForm] = useState({ label: '', start_date: '', end_date: '', base_income: '' })

  useEffect(() => {
    getCategories().then(setCategories)
    getCards().then(setCards)
    getPeriods().then(setPeriods)
  }, [])

  async function saveCat(e: React.FormEvent) {
    e.preventDefault()
    if (editingCat) { await updateCategory(editingCat.id, catForm) }
    else { await createCategory(catForm) }
    setCatModal(false)
    getCategories().then(setCategories)
  }

  async function saveCard(e: React.FormEvent) {
    e.preventDefault()
    if (editingCard) { await updateCard(editingCard.id, cardForm) }
    else { await createCard(cardForm) }
    setCardModal(false)
    getCards().then(setCards)
  }

  async function savePeriod(e: React.FormEvent) {
    e.preventDefault()
    await createPeriod({ ...periodForm, base_income: parseFloat(periodForm.base_income) })
    setPeriodModal(false)
    getPeriods().then(setPeriods)
  }

  return (
    <div>
      <Header title="Configuración" />

      <div className={styles.sections}>

        {/* Categorías */}
        <Card>
          <div className={styles.secHead}>
            <h2 className={styles.secTitle}>Categorías</h2>
            <Button size="sm" onClick={() => { setEditingCat(null); setCatForm({ name: '', color: '#01696f' }); setCatModal(true) }}>
              + Agregar
            </Button>
          </div>
          <div className={styles.list}>
            {categories.map(c => (
              <div key={c.id} className={styles.listItem}>
                <span className={styles.dot} style={{ background: c.color }} />
                <span className={styles.itemName}>{c.name}</span>
                <div className={styles.itemActions}>
                  <Button variant="ghost" size="sm" onClick={() => {
                    setEditingCat(c); setCatForm({ name: c.name, color: c.color }); setCatModal(true)
                  }}>Editar</Button>
                  <Button variant="ghost" size="sm" onClick={async () => {
                    if (!confirm('¿Desactivar?')) return
                    await deleteCategory(c.id)
                    getCategories().then(setCategories)
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
            <Button size="sm" onClick={() => { setEditingCard(null); setCardForm({ name: '', last_four: '', bank: '' }); setCardModal(true) }}>
              + Agregar
            </Button>
          </div>
          <div className={styles.list}>
            {cards.length === 0 && <p className={styles.empty}>Sin tarjetas registradas</p>}
            {cards.map(c => (
              <div key={c.id} className={styles.listItem}>
                <span className={styles.itemName}>{c.name}</span>
                {c.last_four && <Badge variant="default">···{c.last_four}</Badge>}
                {c.bank && <span className={styles.muted}>{c.bank}</span>}
                <div className={styles.itemActions}>
                  <Button variant="ghost" size="sm" onClick={() => {
                    setEditingCard(c); setCardForm({ name: c.name, last_four: c.last_four || '', bank: c.bank || '' }); setCardModal(true)
                  }}>Editar</Button>
                  <Button variant="ghost" size="sm" onClick={async () => {
                    if (!confirm('¿Desactivar?')) return
                    await deleteCard(c.id)
                    getCards().then(setCards)
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
            <Button size="sm" onClick={() => { setPeriodForm({ label: '', start_date: '', end_date: '', base_income: '' }); setPeriodModal(true) }}>
              + Nueva quincena
            </Button>
          </div>
          <div className={styles.list}>
            {periods.slice(0, 8).map(p => (
              <div key={p.id} className={styles.listItem}>
                <span className={styles.itemName}>{p.label}</span>
                <span className={styles.muted}>{formatDate(p.start_date)} — {formatDate(p.end_date)}</span>
                {p.is_closed && <Badge variant="warning">Cerrada</Badge>}
                <Button variant="ghost" size="sm" onClick={async () => {
                  if (!confirm('¿Eliminar quincena?')) return
                  await deletePeriod(p.id)
                  getPeriods().then(setPeriods)
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
            <input type="text" value={catForm.name} onChange={e => setCatForm(f => ({ ...f, name: e.target.value }))} required />
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
            <input type="text" value={cardForm.name} onChange={e => setCardForm(f => ({ ...f, name: e.target.value }))} required />
          </FormField>
          <div className={styles.row}>
            <FormField label="Últimos 4 dígitos">
              <input type="text" maxLength={4} value={cardForm.last_four} onChange={e => setCardForm(f => ({ ...f, last_four: e.target.value }))} />
            </FormField>
            <FormField label="Banco">
              <input type="text" value={cardForm.bank} onChange={e => setCardForm(f => ({ ...f, bank: e.target.value }))} />
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
            <input type="text" placeholder="1ra quincena Mayo 2026" value={periodForm.label}
              onChange={e => setPeriodForm(f => ({ ...f, label: e.target.value }))} required />
          </FormField>
          <div className={styles.row}>
            <FormField label="Inicio">
              <input type="date" value={periodForm.start_date} onChange={e => setPeriodForm(f => ({ ...f, start_date: e.target.value }))} required />
            </FormField>
            <FormField label="Fin">
              <input type="date" value={periodForm.end_date} onChange={e => setPeriodForm(f => ({ ...f, end_date: e.target.value }))} required />
            </FormField>
          </div>
          <FormField label="Ingreso base">
            <input type="number" min="0" value={periodForm.base_income}
              onChange={e => setPeriodForm(f => ({ ...f, base_income: e.target.value }))} required />
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
